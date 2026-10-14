package com.example.mobile_app

import android.app.Application

/**
 * Runs in both the main process and `:unity`. Registers Unity session
 * delivery in whichever process hosts the Unity activity.
 */
class PhysiVLabApp : Application() {
    override fun onCreate() {
        super.onCreate()
        UnityBridge.attach(this)
    }
}
