package com.example.mobile_app

import android.app.Activity
import android.os.Handler
import android.os.Looper
import io.flutter.plugin.common.MethodChannel

/**
 * Same-process bridge between the Flutter host and the Unity library.
 * Unity calls [onResult] via AndroidJavaClass; Flutter reads the session
 * through [takePendingSession] / MethodChannel events.
 */
object UnityBridge {
    @Volatile
    var pendingSession: String? = null

    @Volatile
    var pendingResult: String? = null

    @Volatile
    var unityActivity: Activity? = null

    var channel: MethodChannel? = null

    private val main = Handler(Looper.getMainLooper())

    @JvmStatic
    fun deliverSession(sessionJson: String) {
        main.postDelayed({
            try {
                val unityPlayer = Class.forName("com.unity3d.player.UnityPlayer")
                val send = unityPlayer.getMethod(
                    "UnitySendMessage",
                    String::class.java,
                    String::class.java,
                    String::class.java,
                )
                send.invoke(null, "FlutterBridge", "ReceiveSession", sessionJson)
            } catch (_: Exception) {
            }
        }, 1200)
    }

    @JvmStatic
    fun takePendingSession(): String? {
        val json = pendingSession
        pendingSession = null
        return json
    }

    @JvmStatic
    fun takePendingResult(): String? {
        val json = pendingResult
        pendingResult = null
        return json
    }

    @JvmStatic
    fun onResult(json: String) {
        pendingResult = json
        main.post {
            channel?.invokeMethod("onPracticalCompleted", json)
            finishUnityActivity()
        }
    }

    private fun finishUnityActivity() {
        try {
            val unityPlayer = Class.forName("com.unity3d.player.UnityPlayer")
            val activity = unityPlayer.getField("currentActivity").get(null) as? Activity
            activity?.finish()
        } catch (_: Exception) {
            unityActivity?.finish()
        }
        unityActivity = null
    }

    @JvmStatic
    fun onCancelled() {
        main.post {
            channel?.invokeMethod("onPracticalCancelled", null)
            unityActivity = null
        }
    }
}
