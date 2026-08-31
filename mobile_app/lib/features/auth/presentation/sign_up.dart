import 'package:flutter/material.dart';
import 'package:firebase_auth/firebase_auth.dart';
import 'package:cloud_firestore/cloud_firestore.dart';

import '../../../core/api/api_client.dart';
import '../data/auth_repository.dart';

class SignupScreen extends StatefulWidget {
  const SignupScreen({super.key});

  @override
  State<SignupScreen> createState() => _SignupScreenState();
}

class _SignupScreenState extends State<SignupScreen> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _fieldBorder = Color(0xFFE0E0E0);

  final _repo = AuthRepository();
  final _nameController     = TextEditingController();
  final _emailController    = TextEditingController();
  final _passwordController = TextEditingController();
  final _confirmController  = TextEditingController();

  bool _agreedToTerms  = false;
  bool _isLoading      = false;
  bool _obscurePass    = true;
  bool _obscureConfirm = true;
  int? _selectedGrade; // stored as int: 9, 10, or 11

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    _confirmController.dispose();
    super.dispose();
  }

  Future<void> _signUp() async {
    final fullName = _nameController.text.trim();
    final email    = _emailController.text.trim();
    final password = _passwordController.text;
    final confirm  = _confirmController.text;

    if (fullName.isEmpty || email.isEmpty || password.isEmpty || confirm.isEmpty) {
      _showError('Please fill in all fields.');
      return;
    }
    if (password != confirm) {
      _showError('Passwords do not match.');
      return;
    }
    if (password.length < 6) {
      _showError('Password must be at least 6 characters.');
      return;
    }
    if (_selectedGrade == null) {
      _showError('Please select your grade.');
      return;
    }
    final selectedGrade = _selectedGrade!;
    if (!_agreedToTerms) {
      _showError('Please agree to the Terms & Privacy Policy.');
      return;
    }

    setState(() => _isLoading = true);
    try {
      await _repo.signUp(
        fullName: fullName,
        email: email,
        password: password,
        currentGrade: selectedGrade,
      );
      if (!mounted) return;
      Navigator.of(context).pushReplacementNamed('/home');
    } on ApiException catch (error) {
      if (!mounted) return;
      _showError(error.message);
    } catch (error) {
      if (!mounted) return;
      _showError(error.toString());
    } finally {
      if (mounted) setState(() => _isLoading = false);
    }
  }

  void _showError(String message) {
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(content: Text(message), backgroundColor: Colors.red),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.symmetric(horizontal: 30),
          child: Column(
            children: [
              const SizedBox(height: 40),
              Container(
                padding: const EdgeInsets.all(12),
                decoration: BoxDecoration(
                  color: const Color(0xFFE8F1FF),
                  borderRadius: BorderRadius.circular(12),
                ),
                child: const Icon(Icons.science, color: Color(0xFF0056D2), size: 40),
              ),
              const SizedBox(height: 12),
              const Text('PhysicsLab',
                  style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold)),
              const SizedBox(height: 30),
              const Text('Create Your Account',
                  textAlign: TextAlign.center,
                  style: TextStyle(fontSize: 28, fontWeight: FontWeight.bold, color: _primaryBlue)),
              const SizedBox(height: 10),
              const Text('Start learning physics in a fun and\ninteractive way.',
                  textAlign: TextAlign.center,
                  style: TextStyle(color: Colors.grey, fontSize: 16)),
              const SizedBox(height: 40),
              _buildTextField(_nameController, Icons.person_outline, 'Full Name'),
              const SizedBox(height: 15),
              _buildTextField(_emailController, Icons.email_outlined, 'Email Address',
                  keyboardType: TextInputType.emailAddress),
              const SizedBox(height: 15),
              _buildTextField(_passwordController, Icons.lock_outline, 'Password',
                  isPassword: true, obscure: _obscurePass,
                  onToggle: () => setState(() => _obscurePass = !_obscurePass)),
              const SizedBox(height: 15),
              _buildTextField(_confirmController, Icons.lock_outline, 'Confirm Password',
                  isPassword: true, obscure: _obscureConfirm,
                  onToggle: () => setState(() => _obscureConfirm = !_obscureConfirm)),
              const SizedBox(height: 15),
              _buildGradeDropdown(),
              const SizedBox(height: 10),
              Row(
                children: [
                  Checkbox(
                    value: _agreedToTerms,
                    onChanged: (val) => setState(() => _agreedToTerms = val ?? false),
                    activeColor: _primaryBlue,
                    checkColor: Colors.white,
                    side: const BorderSide(color: _primaryBlue, width: 1.5),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(4)),
                  ),
                  const Expanded(
                    child: Text.rich(TextSpan(text: 'I agree to the ', children: [
                      TextSpan(text: 'Terms', style: TextStyle(color: _primaryBlue)),
                      TextSpan(text: ' & '),
                      TextSpan(text: 'Privacy Policy', style: TextStyle(color: _primaryBlue)),
                    ])),
                  ),
                ],
              ),
              const SizedBox(height: 30),
              SizedBox(
                width: double.infinity,
                height: 55,
                child: ElevatedButton(
                  onPressed: _isLoading ? null : _signUp,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: _primaryBlue,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(30)),
                  ),
                  child: _isLoading
                      ? const SizedBox(height: 22, width: 22,
                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                      : const Text('Sign Up', style: TextStyle(fontSize: 18, color: Colors.white)),
                ),
              ),
              const SizedBox(height: 30),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('Already have an account? '),
                  GestureDetector(
                    onTap: () => Navigator.of(context).maybePop(),
                    child: const Text('Log In',
                        style: TextStyle(color: _primaryBlue, fontWeight: FontWeight.bold)),
                  ),
                ],
              ),
              const SizedBox(height: 20),
            ],
          ),
        ),
      ),
    );
  }

  Widget _buildTextField(
    TextEditingController controller,
    IconData icon,
    String hint, {
    bool isPassword = false,
    bool obscure = false,
    VoidCallback? onToggle,
    TextInputType? keyboardType,
  }) {
    return TextFormField(
      controller: controller,
      obscureText: isPassword ? obscure : false,
      keyboardType: keyboardType,
      cursorColor: _primaryBlue,
      decoration: InputDecoration(
        prefixIcon: Icon(icon, color: Colors.grey),
        hintText: hint,
        hintStyle: const TextStyle(color: Colors.grey),
        filled: true,
        fillColor: const Color(0xFFF8F9FE),
        suffixIcon: isPassword
            ? GestureDetector(
                onTap: onToggle,
                child: Icon(obscure ? Icons.visibility_off_outlined : Icons.visibility_outlined,
                    color: Colors.grey),
              )
            : null,
        border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _fieldBorder)),
        enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _fieldBorder)),
        focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _primaryBlue, width: 2)),
      ),
    );
  }

  Widget _buildGradeDropdown() {
    return DropdownButtonFormField<int>(
      value: _selectedGrade,
      isExpanded: true,
      focusColor: Colors.transparent,
      dropdownColor: Colors.white,
      iconEnabledColor: _primaryBlue,
      decoration: InputDecoration(
        filled: true,
        fillColor: const Color(0xFFF8F9FE),
        contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
        border: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _fieldBorder)),
        enabledBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _fieldBorder)),
        focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(12),
            borderSide: const BorderSide(color: _primaryBlue, width: 2)),
      ),
      hint: const Row(children: [
        Icon(Icons.school_outlined, color: Colors.grey),
        SizedBox(width: 10),
        Text('Select Grade', style: TextStyle(color: Colors.grey)),
      ]),
      items: [9, 10, 11].map((int grade) {
        return DropdownMenuItem<int>(
          value: grade,
          child: Row(children: [
            const Icon(Icons.school, size: 18, color: Color(0xFF2196F3)),
            const SizedBox(width: 8),
            Text('Grade $grade'),
          ]),
        );
      }).toList(),
      onChanged: (val) => setState(() => _selectedGrade = val),
    );
  }
}
