package com.example.mobile_app

import android.app.Activity
import android.app.Application
import android.content.Context
import android.content.Intent
import android.os.Bundle
import android.os.Handler
import android.os.Looper
import android.util.Log
import io.flutter.plugin.common.MethodChannel
import java.io.File

/**
 * Flutter (main process) ↔ Unity (`:unity` process) bridge.
 * Session JSON is written to a shared file; UnitySendMessage is only safe from
 * the `:unity` process after native libs are loaded.
 */
object UnityBridge {
    const val EXTRA_RESULT = "unity_pending_result"
    const val EXTRA_SESSION = "flutter_session"

    private const val TAG = "UnityBridge"
    private const val SESSION_FILE = "unity_pending_session.json"
    private const val RESULT_FILE = "unity_pending_result.json"

    @Volatile
    var pendingSession: String? = null

    @Volatile
    var pendingResult: String? = null

    @Volatile
    var unityActivity: Activity? = null

    var channel: MethodChannel? = null

    private val main = Handler(Looper.getMainLooper())
    private var lifecycleRegistered = false
    private var app: Application? = null
    private var deliverToken = 0

    @Volatile
    private var lastDeliveredSession: String? = null

    @JvmStatic
    fun attach(application: Application) {
        app = application
        registerLifecycle(application)
    }

    fun registerLifecycle(application: Application) {
        app = application
        if (lifecycleRegistered) return
        lifecycleRegistered = true
        application.registerActivityLifecycleCallbacks(object : Application.ActivityLifecycleCallbacks {
            override fun onActivityCreated(activity: Activity, savedInstanceState: Bundle?) {
                if (isUnityHost(activity)) {
                    unityActivity = activity
                    maybeDeliverFromActivity(activity)
                }
            }

            override fun onActivityStarted(activity: Activity) {
                if (isUnityHost(activity)) {
                    unityActivity = activity
                    maybeDeliverFromActivity(activity)
                }
            }

            override fun onActivityResumed(activity: Activity) {
                if (isUnityHost(activity)) {
                    unityActivity = activity
                    maybeDeliverFromActivity(activity)
                }
            }

            override fun onActivityPaused(activity: Activity) {}
            override fun onActivityStopped(activity: Activity) {}
            override fun onActivitySaveInstanceState(activity: Activity, outState: Bundle) {}
            override fun onActivityDestroyed(activity: Activity) {
                if (unityActivity === activity) unityActivity = null
            }
        })
    }

    private fun maybeDeliverFromActivity(activity: Activity) {
        val fromIntent = activity.intent?.getStringExtra(EXTRA_SESSION)
        val session = when {
            !fromIntent.isNullOrBlank() -> fromIntent
            else -> peekPendingSession()
        }
        if (session.isNullOrBlank()) return
        if (session == lastDeliveredSession) return
        lastDeliveredSession = session
        deliverSessionInUnityProcess(session)
    }

    @JvmStatic
    fun stashSession(sessionJson: String) {
        pendingSession = sessionJson
        writeFile(SESSION_FILE, sessionJson)
    }

    /** Flutter/main process: only persist the session. Do not UnitySendMessage here. */
    @JvmStatic
    fun deliverSession(sessionJson: String) {
        stashSession(sessionJson)
    }

    /** Call only from the `:unity` process after natives are loaded. */
    @JvmStatic
    fun deliverSessionInUnityProcess(sessionJson: String) {
        stashSession(sessionJson)
        val token = ++deliverToken
        fun attempt(remaining: Int, delayMs: Long) {
            if (token != deliverToken) return
            main.postDelayed({
                if (token != deliverToken) return@postDelayed
                if (trySendReceiveSession(sessionJson)) {
                    Log.i(TAG, "ReceiveSession delivered")
                    return@postDelayed
                }
                if (remaining > 0) {
                    attempt(remaining - 1, (delayMs + 400).coerceAtMost(2500L))
                } else {
                    Log.w(TAG, "ReceiveSession not delivered; Unity must read file/intent")
                }
            }, delayMs)
        }
        attempt(10, 300)
    }

    @JvmStatic
    fun peekPendingSession(): String? {
        val memory = pendingSession
        if (!memory.isNullOrBlank()) return memory
        return readFile(SESSION_FILE)?.ifBlank { null }
    }

    @JvmStatic
    fun takePendingSession(): String? {
        val json = peekPendingSession()
        return json?.ifBlank { null }
    }

    @JvmStatic
    fun clearPendingSession() {
        pendingSession = null
        deleteFile(SESSION_FILE)
    }

    @JvmStatic
    fun takePendingResult(): String? {
        val json = pendingResult ?: readFile(RESULT_FILE)
        pendingResult = null
        deleteFile(RESULT_FILE)
        return json?.ifBlank { null }
    }

    @JvmStatic
    fun onResult(json: String) {
        pendingResult = json
        writeFile(RESULT_FILE, json)
        clearPendingSession()
        main.post {
            channel?.invokeMethod("onPracticalCompleted", json)
            returnToFlutter(json)
        }
    }

    fun finishUnityActivity() {
        returnToFlutter(pendingResult ?: readFile(RESULT_FILE))
    }

    @JvmStatic
    @JvmOverloads
    fun returnToFlutter(resultJson: String? = null) {
        val from = unityActivity ?: currentUnityActivity()
        val context: Context = from ?: app ?: return
        val intent = Intent(context, MainActivity::class.java).apply {
            addFlags(
                Intent.FLAG_ACTIVITY_NEW_TASK or
                    Intent.FLAG_ACTIVITY_REORDER_TO_FRONT or
                    Intent.FLAG_ACTIVITY_SINGLE_TOP or
                    Intent.FLAG_ACTIVITY_CLEAR_TOP,
            )
            val json = resultJson ?: pendingResult ?: readFile(RESULT_FILE)
            if (!json.isNullOrBlank()) {
                putExtra(EXTRA_RESULT, json)
            }
        }
        context.startActivity(intent)
    }

    fun acceptIncoming(intent: Intent?) {
        val json = intent?.getStringExtra(EXTRA_RESULT) ?: return
        if (json.isBlank()) return
        pendingResult = json
        writeFile(RESULT_FILE, json)
        main.post {
            channel?.invokeMethod("onPracticalCompleted", json)
        }
    }

    @JvmStatic
    fun onCancelled() {
        main.post {
            channel?.invokeMethod("onPracticalCancelled", null)
            returnToFlutter()
        }
    }

    private fun trySendReceiveSession(sessionJson: String): Boolean {
        return try {
            val unityPlayer = Class.forName("com.unity3d.player.UnityPlayer")
            val send = unityPlayer.getMethod(
                "UnitySendMessage",
                String::class.java,
                String::class.java,
                String::class.java,
            )
            send.invoke(null, "FlutterBridge", "ReceiveSession", sessionJson)
            true
        } catch (error: Exception) {
            Log.w(TAG, "UnitySendMessage failed: ${error.message}")
            false
        }
    }

    private fun writeFile(name: String, value: String) {
        try {
            val dir = app?.filesDir ?: unityActivity?.filesDir ?: return
            File(dir, name).writeText(value)
        } catch (_: Exception) {
        }
    }

    private fun readFile(name: String): String? {
        return try {
            val dir = app?.filesDir ?: unityActivity?.filesDir ?: return null
            val file = File(dir, name)
            if (file.exists()) file.readText() else null
        } catch (_: Exception) {
            null
        }
    }

    private fun deleteFile(name: String) {
        try {
            val dir = app?.filesDir ?: unityActivity?.filesDir ?: return
            File(dir, name).delete()
        } catch (_: Exception) {
        }
    }

    private fun currentUnityActivity(): Activity? {
        return try {
            val unityPlayer = Class.forName("com.unity3d.player.UnityPlayer")
            unityPlayer.getField("currentActivity").get(null) as? Activity
        } catch (_: Exception) {
            null
        }
    }

    private fun isUnityHost(activity: Activity): Boolean {
        val name = activity.javaClass.name
        return name.contains("unity3d.player", ignoreCase = true) ||
            name.contains("UnityPlayer", ignoreCase = true)
    }
}
