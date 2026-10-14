// Required for firebase_messaging to work on Flutter Web (both foreground
// token retrieval and background notification display go through this
// service worker). Config matches lib/firebase_options.dart's `web` block.

importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.14.1/firebase-messaging-compat.js');

firebase.initializeApp({
  apiKey: 'AIzaSyB9PuIPzqUAFxa0N3OQ6te5p33RkL2aaY0',
  appId: '1:855512404287:web:c6bec4384f307154a1038a',
  messagingSenderId: '855512404287',
  projectId: 'physics-learning-platform',
  authDomain: 'physics-learning-platform.firebaseapp.com',
  storageBucket: 'physics-learning-platform.firebasestorage.app',
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
  const title = payload.notification?.title || 'PhysicsLab';
  const options = {
    body: payload.notification?.body,
    icon: '/icons/Icon-192.png',
  };
  self.registration.showNotification(title, options);
});
