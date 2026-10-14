import 'dart:io';

Future<List<String>> extraLanHosts() async {
  final hosts = <String>{};
  try {
    final interfaces = await NetworkInterface.list(
      type: InternetAddressType.IPv4,
      includeLinkLocal: false,
    );
    for (final interface in interfaces) {
      for (final addr in interface.addresses) {
        if (addr.isLoopback) continue;
        final parts = addr.address.split('.');
        if (parts.length != 4) continue;
        hosts.add('${parts[0]}.${parts[1]}.${parts[2]}.1');
        hosts.add('${parts[0]}.${parts[1]}.${parts[2]}.53');
      }
    }
  } catch (_) {}
  return hosts.toList();
}
