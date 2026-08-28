import 'package:firebase_auth/firebase_auth.dart';

import '../../../core/api/api_client.dart';

class AuthRepository {
  AuthRepository({ApiClient? api, FirebaseAuth? auth})
      : _api = api ?? ApiClient(),
        _auth = auth ?? FirebaseAuth.instance;

  final ApiClient _api;
  final FirebaseAuth _auth;

  Future<void> signUp({
    required String fullName,
    required String email,
    required String password,
    required int currentGrade,
  }) async {
    await _api.postPublic('/auth/signup', body: {
      'fullName': fullName,
      'email': email,
      'password': password,
      'currentGrade': currentGrade,
      'enrollmentYear': DateTime.now().year,
    });

    final cred = await _auth.signInWithEmailAndPassword(
      email: email,
      password: password,
    );
    await cred.user?.getIdToken(true);
  }

  Future<void> signIn({
    required String email,
    required String password,
  }) async {
    final cred = await _auth.signInWithEmailAndPassword(
      email: email,
      password: password,
    );
    await cred.user?.getIdToken(true);
  }
}
