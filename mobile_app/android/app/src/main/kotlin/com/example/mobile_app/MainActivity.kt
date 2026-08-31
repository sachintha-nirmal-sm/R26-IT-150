package com.example.mobile_app

import android.content.Intent
import android.os.Bundle
import io.flutter.embedding.android.FlutterActivity
import io.flutter.embedding.engine.FlutterEngine
import io.flutter.plugin.common.MethodChannel

class MainActivity : FlutterActivity() {
    private val channelName = "com.example.mobile_app/unity_lab"

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        UnityBridge.attach(application)
        UnityBridge.acceptIncoming(intent)
    }

    override fun onNewIntent(intent: Intent) {
        super.onNewIntent(intent)
        setIntent(intent)
        UnityBridge.acceptIncoming(intent)
    }

    override fun onResume() {
        super.onResume()
        UnityBridge.acceptIncoming(intent)
    }

    override fun configureFlutterEngine(flutterEngine: FlutterEngine) {
        super.configureFlutterEngine(flutterEngine)
        val channel = MethodChannel(flutterEngine.dartExecutor.binaryMessenger, channelName)
        UnityBridge.channel = channel
        channel.setMethodCallHandler { call, result ->
            when (call.method) {
                "isUnityAvailable" -> result.success(isUnityAvailable())
                "startPractical" -> {
                    val sessionJson = call.argument<String>("sessionJson")
                    if (sessionJson.isNullOrBlank()) {
                        result.error("ARG", "sessionJson is required", null)
                    } else {
                        result.success(startUnity(sessionJson))
                    }
                }
                "takePendingResult" -> result.success(UnityBridge.takePendingResult())
                "unloadUnity" -> {
                    UnityBridge.finishUnityActivity()
                    result.success(true)
                }
                else -> result.notImplemented()
            }
        }
    }

    private fun isUnityAvailable(): Boolean {
        return resolveUnityActivity() != null
    }

    private fun startUnity(sessionJson: String): Boolean {
        val activityClass = resolveUnityActivity() ?: return false
        UnityBridge.pendingResult = null
        UnityBridge.deliverSession(sessionJson)
        val intent = Intent(this, activityClass).apply {
            putExtra(UnityBridge.EXTRA_SESSION, sessionJson)
            addFlags(Intent.FLAG_ACTIVITY_NEW_TASK or Intent.FLAG_ACTIVITY_SINGLE_TOP or Intent.FLAG_ACTIVITY_REORDER_TO_FRONT)
        }
        startActivity(intent)
        return true
    }

    private fun resolveUnityActivity(): Class<*>? {
        val names = arrayOf(
            "com.unity3d.player.UnityPlayerGameActivity",
            "com.unity3d.player.UnityPlayerActivity",
        )
        for (name in names) {
            try {
                return Class.forName(name)
            } catch (_: ClassNotFoundException) {
            }
        }
        return null
    }
}
