import 'package:flutter/material.dart';

class SignupScreen extends StatefulWidget {
  const SignupScreen({super.key});

  @override
  State<SignupScreen> createState() => _SignupScreenState();
}

class _SignupScreenState extends State<SignupScreen> {
  static const Color _primaryBlue = Color(0xFF2196F3);
  static const Color _fieldBorder = Color(0xFFE0E0E0);

  bool _agreedToTerms = false;
  String? _selectedGrade;

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
                child: const Icon(
                  Icons.science,
                  color: Color(0xFF0056D2),
                  size: 40,
                ),
              ),
              const SizedBox(height: 12),
              const Text(
                'PhysicsLab',
                style: TextStyle(fontSize: 22, fontWeight: FontWeight.bold),
              ),
              const SizedBox(height: 30),
              const Text(
                'Create Your Account',
                textAlign: TextAlign.center,
                style: TextStyle(
                  fontSize: 28,
                  fontWeight: FontWeight.bold,
                  color: _primaryBlue,
                ),
              ),
              const SizedBox(height: 10),
              const Text(
                'Start learning physics in a fun and\ninteractive way.',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey, fontSize: 16),
              ),
              const SizedBox(height: 40),
              _buildTextField(Icons.person_outline, 'Full Name'),
              const SizedBox(height: 15),
              _buildTextField(Icons.email_outlined, 'Email Address'),
              const SizedBox(height: 15),
              _buildTextField(
                Icons.lock_outline,
                'Password',
                isPassword: true,
              ),
              const SizedBox(height: 15),
              _buildTextField(
                Icons.lock_outline,
                'Confirm Password',
                isPassword: true,
              ),
              const SizedBox(height: 15),
              _buildDropdownField(),
              const SizedBox(height: 10),
              Row(
                children: [
                  Checkbox(
                    value: _agreedToTerms,
                    onChanged: (val) {
                      setState(() => _agreedToTerms = val ?? false);
                    },
                    activeColor: _primaryBlue,
                    checkColor: Colors.white,
                    side: const BorderSide(color: _primaryBlue, width: 1.5),
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(4),
                    ),
                  ),
                  const Expanded(
                    child: Text.rich(
                      TextSpan(
                        text: 'I agree to the ',
                        children: [
                          TextSpan(
                            text: 'Terms',
                            style: TextStyle(color: _primaryBlue),
                          ),
                          TextSpan(text: ' & '),
                          TextSpan(
                            text: 'Privacy Policy',
                            style: TextStyle(color: _primaryBlue),
                          ),
                        ],
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 30),
              SizedBox(
                width: double.infinity,
                height: 55,
                child: ElevatedButton(
                  onPressed: () {
                    // Pass selected grade to home page
                    Navigator.of(context).pushReplacementNamed(
                      '/home',
                      arguments: {'grade': _selectedGrade ?? 'Grade 10'},
                    );
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: _primaryBlue,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(30),
                    ),
                  ),
                  child: const Text(
                    'Sign Up',
                    style: TextStyle(fontSize: 18, color: Colors.white),
                  ),
                ),
              ),
              const SizedBox(height: 30),
              Row(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  const Text('Already have an account? '),
                  GestureDetector(
                    onTap: () => Navigator.of(context).maybePop(),
                    child: const Text(
                      'Log In',
                      style: TextStyle(
                        color: _primaryBlue,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
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
    IconData icon,
    String hint, {
    bool isPassword = false,
  }) {
    return TextFormField(
      obscureText: isPassword,
      cursorColor: _primaryBlue,
      decoration: InputDecoration(
        prefixIcon: Icon(icon, color: Colors.grey),
        hintText: hint,
        hintStyle: const TextStyle(color: Colors.grey),
        filled: true,
        fillColor: const Color(0xFFF8F9FE),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _fieldBorder),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _fieldBorder),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _primaryBlue, width: 2),
        ),
      ),
    );
  }

  Widget _buildDropdownField() {
    return DropdownButtonFormField<String>(
      value: _selectedGrade,
      isExpanded: true,
      focusColor: Colors.transparent,
      dropdownColor: Colors.white,
      iconEnabledColor: _primaryBlue,
      decoration: InputDecoration(
        filled: true,
        fillColor: const Color(0xFFF8F9FE),
        contentPadding:
            const EdgeInsets.symmetric(horizontal: 12, vertical: 14),
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _fieldBorder),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _fieldBorder),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(12),
          borderSide: const BorderSide(color: _primaryBlue, width: 2),
        ),
      ),
      hint: const Row(
        children: [
          Icon(Icons.school_outlined, color: Colors.grey),
          SizedBox(width: 10),
          Text('Select Grade', style: TextStyle(color: Colors.grey)),
        ],
      ),
      items: ['Grade 9', 'Grade 10', 'Grade 11'].map((String value) {
        return DropdownMenuItem<String>(
          value: value,
          child: Row(
            children: [
              const Icon(Icons.school, size: 18, color: Color(0xFF2196F3)),
              const SizedBox(width: 8),
              Text(value),
            ],
          ),
        );
      }).toList(),
      onChanged: (val) => setState(() => _selectedGrade = val),
    );
  }
}
