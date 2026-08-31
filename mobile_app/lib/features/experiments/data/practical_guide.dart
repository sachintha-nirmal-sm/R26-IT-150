import 'package:flutter/material.dart';

import 'practical.dart';

class PracticalGuide {
  const PracticalGuide({
    required this.headline,
    required this.goal,
    required this.icon,
    required this.color,
    required this.accent,
    required this.kit,
    required this.steps,
    required this.tip,
  });

  final String headline;
  final String goal;
  final IconData icon;
  final Color color;
  final Color accent;
  final List<String> kit;
  final List<String> steps;
  final String tip;

  static PracticalGuide forPractical(String id, {String title = ''}) {
    final byId = _guides[id];
    if (byId != null) return byId;
    final key = LocalPracticals.matchTopicId(title, id);
    if (key != null && _guides.containsKey(key)) return _guides[key]!;
    return PracticalGuide(
      headline: title.isEmpty ? 'Virtual physics lab' : title,
      goal: 'Follow the on-screen lab and record your observations.',
      icon: Icons.science,
      color: const Color(0xFF2F80ED),
      accent: const Color(0xFF6EC6FF),
      kit: const ['Lab tools', 'Observation table'],
      steps: const [
        'Open the lab and read the first instruction.',
        'Complete each step before moving on.',
        'Check your answers, then finish to save your score.',
      ],
      tip: 'Trial is for practice. Start is timed and saves to your profile.',
    );
  }
}

const _guides = <String, PracticalGuide>{
  'grade9_force_basic': PracticalGuide(
    headline: 'Feel the pull of gravity',
    goal: 'Hang objects on a spring balance and compare weight with mass.',
    icon: Icons.fitness_center,
    color: Color(0xFF1565C0),
    accent: Color(0xFF90CAF9),
    kit: ['Spring balance', 'Hook', 'Known masses'],
    steps: [
      'Hang each object from the spring balance.',
      'Read the weight in newtons carefully.',
      'Use W = mg to find mass, then check the table.',
    ],
    tip: 'Keep the balance still before you read the scale.',
  ),
  'grade9_density_water': PracticalGuide(
    headline: 'How dense is water?',
    goal: 'Measure mass and volume of water, then calculate density.',
    icon: Icons.water_drop,
    color: Color(0xFF0277BD),
    accent: Color(0xFF81D4FA),
    kit: ['Beaker', 'Measuring cylinder', 'Balance'],
    steps: [
      'Find the mass of a known volume of water.',
      'Record volume from the measuring cylinder.',
      'Calculate density = mass ÷ volume.',
    ],
    tip: 'Read the meniscus at eye level for a fair volume.',
  ),
  'grade9_pressure_solid': PracticalGuide(
    headline: 'Pressure that cuts',
    goal: 'See how a thin wire and sandbags press into a soap bar.',
    icon: Icons.compress,
    color: Color(0xFF6A1B9A),
    accent: Color(0xFFCE93D8),
    kit: ['Soap bar', 'Thin wire', 'Sandbags'],
    steps: [
      'Place the wire on the soap and add the loads.',
      'Watch how far the wire sinks for each setup.',
      'Link a smaller area with a bigger pressure.',
    ],
    tip: 'Same force on a smaller area means more pressure.',
  ),
  'grade9_reflection_prism': PracticalGuide(
    headline: 'Catch a rainbow',
    goal: 'Send a thin white beam through a glass prism and name ROYGBIV.',
    icon: Icons.blur_on,
    color: Color(0xFFAD1457),
    accent: Color(0xFFF48FB1),
    kit: ['Ray box', 'Glass prism', 'White screen'],
    steps: [
      'Pick the ray box, prism and screen.',
      'Aim a thin white beam at the prism.',
      'Name the colours in order: R O Y G B I V.',
    ],
    tip: 'A narrow beam makes the spectrum easier to see.',
  ),
  'grade9_lever_15_1': PracticalGuide(
    headline: 'Balance the lever',
    goal: 'Set a first-class lever and see how effort changes with distance.',
    icon: Icons.swap_horiz,
    color: Color(0xFFEF6C00),
    accent: Color(0xFFFFCC80),
    kit: ['Ruler', 'Fulcrum', 'Load & effort'],
    steps: [
      'Place the fulcrum and hang the load.',
      'Move the effort until the lever balances.',
      'Record effort for each load distance.',
    ],
    tip: 'A longer effort arm means a smaller effort.',
  ),
  'grade10_hydrostatic_pressure': PracticalGuide(
    headline: 'Upthrust in water',
    goal: 'Use a Eureka can and spring balance to measure upthrust.',
    icon: Icons.waves,
    color: Color(0xFF00695C),
    accent: Color(0xFF80CBC4),
    kit: ['Eureka can', 'Spring balance', 'Solid object'],
    steps: [
      'Weigh the object in air, then in water.',
      'Collect the overflow from the Eureka can.',
      'Compare weight lost with the weight of displaced water.',
    ],
    tip: 'Fill the can to the spout before you lower the object.',
  ),
  'grade10_work_energy_power': PracticalGuide(
    headline: 'Energy that does work',
    goal: 'Drop a weight onto clay and relate height, work and power.',
    icon: Icons.bolt,
    color: Color(0xFFE65100),
    accent: Color(0xFFFFCC80),
    kit: ['Falling mass', 'Clay', 'Metre rule'],
    steps: [
      'Drop the mass from different heights onto clay.',
      'Measure how deep the dent is each time.',
      'Connect height with potential energy, work and power.',
    ],
    tip: 'A greater height stores more gravitational potential energy.',
  ),
  'grade10_current_electricity': PracticalGuide(
    headline: 'Build the circuit',
    goal: 'Connect two dry cells in series, parallel and opposing ways.',
    icon: Icons.electrical_services,
    color: Color(0xFFE53935),
    accent: Color(0xFFFFAB91),
    kit: ['Dry cells', 'Bulb', 'Ammeter & wires'],
    steps: [
      'Build a simple circuit with two cells.',
      'Try series, then parallel, then opposing cells.',
      'Watch how the bulb brightness and current change.',
    ],
    tip: 'Opposing cells fight each other — the bulb may go dim.',
  ),
  'grade10_motion_straight_line': PracticalGuide(
    headline: 'Race along a line',
    goal: 'Time a toy car and compare distance, speed and acceleration.',
    icon: Icons.directions_car,
    color: Color(0xFF283593),
    accent: Color(0xFF9FA8DA),
    kit: ['Toy car', 'Track', 'Stopwatch'],
    steps: [
      'Release the car and start the timer.',
      'Note distance and time for each run.',
      'Find speed, then see if the motion is accelerating.',
    ],
    tip: 'Start the timer at the same mark every run.',
  ),
  'grade10_newtons_laws': PracticalGuide(
    headline: "Newton's three laws",
    goal: 'Use a trolley and pulley to explore inertia, F = ma and action–reaction.',
    icon: Icons.speed,
    color: Color(0xFF4527A0),
    accent: Color(0xFFB39DDB),
    kit: ['Trolley', 'Pulley', 'Slotted masses'],
    steps: [
      'Watch the trolley stay still until a force acts.',
      'Add hanging masses and see acceleration change.',
      'Spot action and reaction on the string and trolley.',
    ],
    tip: 'A bigger unbalanced force means a bigger acceleration.',
  ),
  'grade10_friction': PracticalGuide(
    headline: 'Fighting friction',
    goal: 'Pull a wooden block on sandpaper and compare limiting friction.',
    icon: Icons.swipe,
    color: Color(0xFF5D4037),
    accent: Color(0xFFBCAAA4),
    kit: ['Wooden block', 'Sandpaper', 'Newton balance'],
    steps: [
      'Place the block on sandpaper and pull slowly.',
      'Read the force just as the block starts to move.',
      'Repeat for different contact areas.',
    ],
    tip: 'Limiting friction is the pull just before sliding starts.',
  ),
  'grade10_resultant_force': PracticalGuide(
    headline: 'Two forces, one result',
    goal: 'Find the resultant of two forces with spring balances and pulleys.',
    icon: Icons.call_split,
    color: Color(0xFF00838F),
    accent: Color(0xFF80DEEA),
    kit: ['Two spring balances', 'Pulleys', 'Trolley'],
    steps: [
      'Apply two forces at the chosen angles.',
      'Find a third force that keeps the trolley still.',
      'Check that the resultant matches the two forces combined.',
    ],
    tip: 'If the trolley does not move, the three forces are in equilibrium.',
  ),
  'grade10_turning_effect': PracticalGuide(
    headline: 'The turning trick',
    goal: 'Investigate the moment of a force with a pivot and newton balance.',
    icon: Icons.rotate_right,
    color: Color(0xFF2E7D32),
    accent: Color(0xFFA5D6A7),
    kit: ['Clamped stick', 'Pivot', 'Newton balance'],
    steps: [
      'Set the pivot and hang or pull at a marked distance.',
      'Read the force on the newton balance.',
      'Calculate moment = force × perpendicular distance.',
    ],
    tip: 'A force farther from the pivot turns more easily.',
  ),
  'grade10_equilibrium': PracticalGuide(
    headline: 'Keep the ruler still',
    goal: 'Balance a metre ruler under three parallel forces and compare F1 + F2 with W.',
    icon: Icons.balance,
    color: Color(0xFF00897B),
    accent: Color(0xFF80CBC4),
    kit: ['Metre ruler', 'Two spring balances', 'Hangars'],
    steps: [
      'Hang the ruler so it stays horizontal.',
      'Read F1, F2 and the weight W.',
      'Check whether F1 + F2 equals W.',
    ],
    tip: 'Clockwise moments should cancel anticlockwise moments.',
  ),
  'grade11_waves': PracticalGuide(
    headline: 'A wave you can see',
    goal: 'Make a transverse wave on a slinky and watch a ribbon move.',
    icon: Icons.graphic_eq,
    color: Color(0xFF3949AB),
    accent: Color(0xFF9FA8DA),
    kit: ['Slinky', 'Ribbon marker', 'Floor space'],
    steps: [
      'Stretch the slinky and flick one end sideways.',
      'Watch the pulse travel along the coil.',
      'See the ribbon move up and down, not along the slinky.',
    ],
    tip: 'In a transverse wave, particles move at right angles to the energy.',
  ),
  'grade11_geometrical_optics': PracticalGuide(
    headline: 'Find the focal length',
    goal: 'Focus a distant object with a concave mirror onto a white screen.',
    icon: Icons.camera_enhance,
    color: Color(0xFF546E7A),
    accent: Color(0xFFB0BEC5),
    kit: ['Concave mirror', 'White screen', 'Metre rule'],
    steps: [
      'Point the mirror at a distant window or lamp.',
      'Move the screen until the image is sharp.',
      'Measure the distance from mirror to screen — that is f.',
    ],
    tip: 'A distant object sends almost parallel rays to the mirror.',
  ),
  'grade11_heat': PracticalGuide(
    headline: 'Watch a liquid grow',
    goal: 'Show expansion of a liquid with a test tube in a warm water bath.',
    icon: Icons.thermostat,
    color: Color(0xFFD84315),
    accent: Color(0xFFFFAB91),
    kit: ['Test tube', 'Thin glass tube', 'Water bath'],
    steps: [
      'Fill the test tube and fit the thin tube.',
      'Warm the bath and watch the liquid rise.',
      'Explain expansion using the particle model.',
    ],
    tip: 'Heat makes particles move farther apart, so the liquid needs more space.',
  ),
  'grade11_power_appliances': PracticalGuide(
    headline: 'Power of home devices',
    goal: 'Measure V and I, then find power, energy and kilowatt-hours.',
    icon: Icons.power,
    color: Color(0xFFC62828),
    accent: Color(0xFFEF9A9A),
    kit: ['Voltmeter', 'Ammeter', 'Appliance'],
    steps: [
      'Read voltage and current for the appliance.',
      'Calculate power P = VI.',
      'Find energy and convert to kWh.',
    ],
    tip: 'A kilowatt-hour is the energy used by 1000 W for one hour.',
  ),
  'grade11_electronics': PracticalGuide(
    headline: 'Which way for the diode?',
    goal: 'Compare forward bias and reverse bias with a battery, switch and bulb.',
    icon: Icons.memory,
    color: Color(0xFF1A237E),
    accent: Color(0xFF7986CB),
    kit: ['Diode', 'Battery', 'Switch & bulb'],
    steps: [
      'Connect the diode so the bulb lights — forward bias.',
      'Reverse the diode and try again.',
      'Explain why current flows only one way.',
    ],
    tip: 'The stripe on a diode marks the cathode — current is blocked that way in reverse bias.',
  ),
};
