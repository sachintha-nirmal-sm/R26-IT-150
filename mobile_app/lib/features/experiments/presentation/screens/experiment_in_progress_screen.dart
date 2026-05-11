import 'package:flutter/material.dart';
import 'dart:async';
import 'experiment_results_screen.dart';

class ExperimentInProgressScreen extends StatefulWidget {
  final bool showTimer;

  const ExperimentInProgressScreen({super.key, this.showTimer = true});

  @override
  State<ExperimentInProgressScreen> createState() =>
      _ExperimentInProgressScreenState();
}

class _ExperimentInProgressScreenState extends State<ExperimentInProgressScreen>
    with TickerProviderStateMixin {
  late Timer _timer;
  late AnimationController _progressController;
  int _elapsedSeconds = 0;
  int _elapsedMilliseconds = 0;
  bool _isPaused = false;
  int _selectedBottomNavIndex = 1;

  final int _totalDurationSeconds = 20 * 60; // 20 minutes

  @override
  void initState() {
    super.initState();
    _startTimer();
    _progressController = AnimationController(
      duration: const Duration(seconds: 20 * 60),
      vsync: this,
    );
    _progressController.forward();
  }

  void _startTimer() {
    _timer = Timer.periodic(const Duration(milliseconds: 10), (timer) {
      if (!_isPaused) {
        setState(() {
          _elapsedMilliseconds += 10;
          if (_elapsedMilliseconds >= 1000) {
            _elapsedSeconds += 1;
            _elapsedMilliseconds = 0;
          }

          // Stop timer at 20 minutes
          if (_elapsedSeconds >= _totalDurationSeconds) {
            _timer.cancel();
          }
        });
      }
    });
  }

  void _pauseTimer() {
    setState(() {
      _isPaused = !_isPaused;
      if (_isPaused) {
        _progressController.stop();
      } else {
        _progressController.forward();
      }
    });
  }

  void _resetTimer() {
    setState(() {
      _elapsedSeconds = 0;
      _elapsedMilliseconds = 0;
      _isPaused = false;
    });
    _progressController.reset();
    _progressController.forward();
  }

  @override
  void dispose() {
    _timer.cancel();
    _progressController.dispose();
    super.dispose();
  }

  String _getFormattedTime() {
    int minutes = _elapsedSeconds ~/ 60;
    int seconds = _elapsedSeconds % 60;
    int centiseconds = _elapsedMilliseconds ~/ 10;

    return '${minutes.toString().padLeft(2, '0')}:${seconds.toString().padLeft(2, '0')}.${centiseconds.toString().padLeft(2, '0')}';
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.white,
      appBar: _buildAppBar(),
      body: SingleChildScrollView(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              // Live Video View
              _buildVideoCard(),
              const SizedBox(height: 24),

              // Timer Section
              if (widget.showTimer) ...[
                _buildTimerSection(),
                const SizedBox(height: 20),
              ],

              // Sensor Status Banner
              _buildSensorStatusBanner(),
              const SizedBox(height: 20),

              // Control Buttons
              _buildControlButtons(),
              const SizedBox(height: 16),

              // Full-width Finish Button
              _buildFinishButton(),
              const SizedBox(height: 20),

              // Instructions Accordion
              _buildInstructionsAccordion(),
              const SizedBox(height: 32),
            ],
          ),
        ),
      ),
      bottomNavigationBar: _buildBottomNavigationBar(),
    );
  }

  PreferredSizeWidget _buildAppBar() {
    return AppBar(
      elevation: 0,
      backgroundColor: Colors.white,
      leading: IconButton(
        icon: const Icon(Icons.arrow_back, color: Color(0xFF2F80ED), size: 28),
        onPressed: () => Navigator.pop(context),
      ),
      title: const Text(
        'Experiment in Progress',
        style: TextStyle(
          color: Color(0xFF2F80ED),
          fontSize: 18,
          fontWeight: FontWeight.w700,
          fontFamily: 'Poppins',
        ),
      ),
      centerTitle: false,
      actions: [
        Container(
          margin: const EdgeInsets.all(12),
          width: 44,
          height: 44,
          decoration: BoxDecoration(
            shape: BoxShape.circle,
            gradient: const LinearGradient(
              colors: [Color(0xFF2F80ED), Color(0xFF1C5ED6)],
              begin: Alignment.topLeft,
              end: Alignment.bottomRight,
            ),
            boxShadow: [
              BoxShadow(
                color: const Color(0xFF2F80ED).withValues(alpha: 0.3),
                blurRadius: 12,
                offset: const Offset(0, 4),
              ),
            ],
          ),
          child: const Center(
            child: Text(
              'A',
              style: TextStyle(
                color: Colors.white,
                fontSize: 18,
                fontWeight: FontWeight.w600,
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildVideoCard() {
    return Card(
      elevation: 4,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(16),
      ),
      child: Container(
        height: 200,
        decoration: BoxDecoration(
          borderRadius: BorderRadius.circular(16),
          color: const Color(0xFF2C3E50),
          image: const DecorationImage(
            image: AssetImage('assets/images/experiment_setup.png'),
            fit: BoxFit.cover,
          ),
        ),
        child: Container(
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(16),
            color: Colors.black.withValues(alpha: 0.3),
          ),
          child: Center(
            child: Container(
              width: 64,
              height: 64,
              decoration: BoxDecoration(
                shape: BoxShape.circle,
                color: Colors.white.withValues(alpha: 0.9),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.2),
                    blurRadius: 12,
                    offset: const Offset(0, 4),
                  ),
                ],
              ),
              child: const Icon(
                Icons.play_arrow,
                color: Color(0xFF2F80ED),
                size: 32,
              ),
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildTimerSection() {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(12),
      ),
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'ELAPSED TIME',
              style: TextStyle(
                fontSize: 11,
                fontWeight: FontWeight.w600,
                color: Color(0xFF999999),
                letterSpacing: 0.5,
                fontFamily: 'Poppins',
              ),
            ),
            const SizedBox(height: 12),
            Row(
              children: [
                Expanded(
                  child: Text(
                    _getFormattedTime(),
                    style: const TextStyle(
                      fontSize: 40,
                      fontWeight: FontWeight.w700,
                      color: Color(0xFF2F80ED),
                      fontFamily: 'Courier New',
                      letterSpacing: 2,
                    ),
                  ),
                ),
                SizedBox(
                  width: 80,
                  height: 80,
                  child: Stack(
                    alignment: Alignment.center,
                    children: [
                      SizedBox(
                        width: 80,
                        height: 80,
                        child: CircularProgressIndicator(
                          value: _elapsedSeconds / _totalDurationSeconds,
                          strokeWidth: 4,
                          valueColor: const AlwaysStoppedAnimation<Color>(
                            Color(0xFF2F80ED),
                          ),
                          backgroundColor: const Color(0xFFE8F0FE),
                        ),
                      ),
                      Container(
                        width: 48,
                        height: 48,
                        decoration: BoxDecoration(
                          shape: BoxShape.circle,
                          color: const Color(0xFFE8F0FE),
                          border: Border.all(
                            color: const Color(0xFF2F80ED),
                            width: 2,
                          ),
                        ),
                        child: const Icon(
                          Icons.schedule,
                          color: Color(0xFF2F80ED),
                          size: 24,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildSensorStatusBanner() {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
      decoration: BoxDecoration(
        color: const Color(0xFF2F80ED),
        borderRadius: BorderRadius.circular(12),
        boxShadow: [
          BoxShadow(
            color: const Color(0xFF2F80ED).withValues(alpha: 0.3),
            blurRadius: 8,
            offset: const Offset(0, 2),
          ),
        ],
      ),
      child: const Row(
        children: [
          Icon(
            Icons.science,
            color: Colors.white,
            size: 24,
          ),
          SizedBox(width: 12),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Velocity Sensor Active',
                  style: TextStyle(
                    fontSize: 14,
                    fontWeight: FontWeight.w600,
                    color: Colors.white,
                    fontFamily: 'Poppins',
                  ),
                ),
                SizedBox(height: 2),
                Text(
                  'Receiving Live Data',
                  style: TextStyle(
                    fontSize: 12,
                    color: Colors.white70,
                    fontFamily: 'Poppins',
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }

  Widget _buildControlButtons() {
    return Row(
      children: [
        Expanded(
          child: ElevatedButton.icon(
            onPressed: _pauseTimer,
            icon: Icon(
              _isPaused ? Icons.play_arrow : Icons.pause,
              size: 20,
            ),
            label: Text(_isPaused ? 'Resume' : 'Pause'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 12),
              backgroundColor: const Color(0xFF2F80ED),
              foregroundColor: Colors.white,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
        ),
        const SizedBox(width: 12),
        Expanded(
          child: ElevatedButton.icon(
            onPressed: _resetTimer,
            icon: const Icon(Icons.refresh, size: 20),
            label: const Text('Reset'),
            style: ElevatedButton.styleFrom(
              padding: const EdgeInsets.symmetric(vertical: 12),
              backgroundColor: const Color(0xFFE0E5EC),
              foregroundColor: Colors.black87,
              shape: RoundedRectangleBorder(
                borderRadius: BorderRadius.circular(8),
              ),
            ),
          ),
        ),
      ],
    );
  }

  Widget _buildFinishButton() {
    return SizedBox(
      width: double.infinity,
      child: ElevatedButton.icon(
        onPressed: () {
          _timer.cancel();
          int finalMinutes = _elapsedSeconds ~/ 60;
          int finalSeconds = _elapsedSeconds % 60;
          String durationStr = "${finalMinutes} min ${finalSeconds} sec";

          Navigator.push(
            context,
            MaterialPageRoute(
              builder: (context) => ExperimentResultsScreen(
                score: 0, // Practical not uploaded yet
                finalDuration: durationStr,
              ),
            ),
          );
        },
        icon: const Icon(Icons.check_circle, size: 22),
        label: const Text('Finish & Analyze Data'),
        style: ElevatedButton.styleFrom(
          padding: const EdgeInsets.symmetric(vertical: 14),
          backgroundColor: const Color(0xFF00695C),
          foregroundColor: Colors.white,
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(8),
          ),
        ),
      ),
    );
  }

  Widget _buildInstructionsAccordion() {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(8),
      ),
      child: Theme(
        data: Theme.of(context).copyWith(dividerColor: Colors.transparent),
        child: const ExpansionTile(
          title: Row(
            children: [
              Icon(
                Icons.info_outline,
                color: Color(0xFF2F80ED),
                size: 20,
              ),
              SizedBox(width: 12),
              Text(
                'View Experiment Instructions',
                style: TextStyle(
                  fontSize: 14,
                  fontWeight: FontWeight.w600,
                  color: Colors.black87,
                  fontFamily: 'Poppins',
                ),
              ),
            ],
          ),
          children: [
            Padding(
              padding: EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    '1. Ensure the sensor is calibrated before starting',
                    style: TextStyle(
                      fontSize: 13,
                      color: Color(0xFF666666),
                      height: 1.5,
                      fontFamily: 'Poppins',
                    ),
                  ),
                  SizedBox(height: 8),
                  Text(
                    '2. Monitor the live data feed continuously',
                    style: TextStyle(
                      fontSize: 13,
                      color: Color(0xFF666666),
                      height: 1.5,
                      fontFamily: 'Poppins',
                    ),
                  ),
                  SizedBox(height: 8),
                  Text(
                    '3. Click Finish when the experiment is complete',
                    style: TextStyle(
                      fontSize: 13,
                      color: Color(0xFF666666),
                      height: 1.5,
                      fontFamily: 'Poppins',
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildBottomNavigationBar() {
    return BottomNavigationBar(
      currentIndex: _selectedBottomNavIndex,
      onTap: (index) {
        setState(() {
          _selectedBottomNavIndex = index;
        });
      },
      type: BottomNavigationBarType.fixed,
      backgroundColor: Colors.white,
      selectedItemColor: const Color(0xFF2F80ED),
      unselectedItemColor: Colors.grey,
      elevation: 8,
      items: const [
        BottomNavigationBarItem(
          icon: Icon(Icons.explore),
          label: 'Explore',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.science),
          label: 'Experiments',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.menu_book),
          label: 'Library',
        ),
        BottomNavigationBarItem(
          icon: Icon(Icons.person),
          label: 'Profile',
        ),
      ],
    );
  }
}
