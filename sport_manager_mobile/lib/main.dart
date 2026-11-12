import 'dart:convert';

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;

void main() {
  runApp(const SportManagerApp());
}

class ApiService {
  ApiService._();

  static final ApiService instance = ApiService._();

  static const String _baseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5084',
  );

  Future<dynamic> _getJson(String path) async {
    final response = await http
        .get(
          Uri.parse('$_baseUrl$path'),
          headers: const {'Accept': 'application/json'},
        )
        .timeout(const Duration(seconds: 10));

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Erreur HTTP ${response.statusCode} sur $path');
    }

    return jsonDecode(response.body);
  }

  Map<String, dynamic> _asMap(dynamic value) {
    if (value is! Map) {
      throw const FormatException('La réponse de l’API n’est pas un objet JSON.');
    }

    return Map<String, dynamic>.from(value);
  }

  Future<List<TeamSummary>> getTeams() async {
    final data = await _getJson('/api/equipes');

    if (data is! List) {
      throw const FormatException('La liste des équipes est invalide.');
    }

    return data
        .map((item) => TeamSummary.fromJson(_asMap(item)))
        .toList();
  }

  Future<List<PlayerInfo>> getPlayers() async {
    final data = await _getJson('/api/joueurs');

    if (data is! List) {
      throw const FormatException('La liste des joueurs est invalide.');
    }

    return data
        .map((item) => PlayerInfo.fromJson(_asMap(item)))
        .toList();
  }

  Future<TeamDetails> getTeamDetails(int id) async {
    return TeamDetails.fromJson(_asMap(await _getJson('/api/equipe/$id')));
  }

  Future<PlayerInfo> getPlayerDetails(int id) async {
    return PlayerInfo.fromJson(_asMap(await _getJson('/api/joueur/$id')));
  }
}

int _asInt(dynamic value) => value is num ? value.toInt() : 0;

class TeamSummary {
  const TeamSummary({
    required this.id,
    required this.name,
    required this.city,
    required this.coachName,
    required this.playerCount,
  });

  final int id;
  final String name;
  final String city;
  final String coachName;
  final int playerCount;

  factory TeamSummary.fromJson(Map<String, dynamic> json) {
    return TeamSummary(
      id: _asInt(json['id']),
      name: json['name'] as String? ?? '',
      city: json['city'] as String? ?? '',
      coachName: json['coachName'] as String? ?? '',
      playerCount: _asInt(json['playerCount']),
    );
  }
}

class TeamDetails {
  const TeamDetails({
    required this.id,
    required this.name,
    required this.city,
    required this.coachName,
    required this.players,
  });

  final int id;
  final String name;
  final String city;
  final String coachName;
  final List<PlayerInfo> players;

  factory TeamDetails.fromJson(Map<String, dynamic> json) {
    final rawPlayers = json['players'];

    if (rawPlayers is! List) {
      throw const FormatException('La liste des joueurs de l’équipe est invalide.');
    }

    return TeamDetails(
      id: _asInt(json['id']),
      name: json['name'] as String? ?? '',
      city: json['city'] as String? ?? '',
      coachName: json['coachName'] as String? ?? '',
      players: rawPlayers
          .map(
            (item) => PlayerInfo.fromJson(
              Map<String, dynamic>.from(item as Map),
            ),
          )
          .toList(),
    );
  }
}

class PlayerInfo {
  const PlayerInfo({
    required this.id,
    required this.firstName,
    required this.lastName,
    required this.fullName,
    required this.age,
    required this.number,
    required this.teamName,
    required this.positionName,
    required this.speed,
    required this.endurance,
    required this.strength,
    required this.technique,
    required this.overallRating,
    required this.isInjured,
  });

  final int id;
  final String firstName;
  final String lastName;
  final String fullName;
  final int age;
  final int number;
  final String teamName;
  final String positionName;
  final int speed;
  final int endurance;
  final int strength;
  final int technique;
  final double overallRating;
  final bool isInjured;

  factory PlayerInfo.fromJson(Map<String, dynamic> json) {
    return PlayerInfo(
      id: _asInt(json['id']),
      firstName: json['firstName'] as String? ?? '',
      lastName: json['lastName'] as String? ?? '',
      fullName: json['fullName'] as String? ?? '',
      age: _asInt(json['age']),
      number: _asInt(json['number']),
      teamName: json['teamName'] as String? ?? '',
      positionName: json['positionName'] as String? ?? '',
      speed: _asInt(json['speed']),
      endurance: _asInt(json['endurance']),
      strength: _asInt(json['strength']),
      technique: _asInt(json['technique']),
      overallRating: (json['overallRating'] as num?)?.toDouble() ?? 0,
      isInjured: json['isInjured'] as bool? ?? false,
    );
  }
}

class SportManagerApp extends StatelessWidget {
  const SportManagerApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Sport Manager',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.green),
        useMaterial3: true,
      ),
      home: const HomePage(),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key});

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int _selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_selectedIndex == 0 ? 'Équipes' : 'Joueurs'),
      ),
      body: IndexedStack(
        index: _selectedIndex,
        children: const [
          TeamsPage(),
          PlayersPage(),
        ],
      ),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _selectedIndex,
        onDestinationSelected: (index) {
          setState(() => _selectedIndex = index);
        },
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.groups_outlined),
            selectedIcon: Icon(Icons.groups),
            label: 'EQUIPES',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline),
            selectedIcon: Icon(Icons.person),
            label: 'JOUEURS',
          ),
        ],
      ),
    );
  }
}

class TeamsPage extends StatefulWidget {
  const TeamsPage({super.key});

  @override
  State<TeamsPage> createState() => _TeamsPageState();
}

class _TeamsPageState extends State<TeamsPage> {
  late Future<List<TeamSummary>> _future;

  @override
  void initState() {
    super.initState();
    _future = ApiService.instance.getTeams();
  }

  void _retry() {
    setState(() => _future = ApiService.instance.getTeams());
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<TeamSummary>>(
      future: _future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return _FailureView(error: snapshot.error!, onRetry: _retry);
        }

        final teams = snapshot.data ?? [];

        if (teams.isEmpty) {
          return const Center(child: Text('Aucune équipe.'));
        }

        return ListView.builder(
          padding: const EdgeInsets.all(12),
          itemCount: teams.length,
          itemBuilder: (context, index) {
            final team = teams[index];

            return Card(
              child: ListTile(
                title: Text(team.name),
                subtitle: Text(
                  '${team.city} · ${team.playerCount} joueur(s)',
                ),
                trailing: const Icon(Icons.chevron_right),
                onTap: () {
                  Navigator.of(context).push(
                    MaterialPageRoute<void>(
                      builder: (_) => TeamDetailsPage(id: team.id),
                    ),
                  );
                },
              ),
            );
          },
        );
      },
    );
  }
}

class PlayersPage extends StatefulWidget {
  const PlayersPage({super.key});

  @override
  State<PlayersPage> createState() => _PlayersPageState();
}

class _PlayersPageState extends State<PlayersPage> {
  late Future<List<PlayerInfo>> _future;

  @override
  void initState() {
    super.initState();
    _future = ApiService.instance.getPlayers();
  }

  void _retry() {
    setState(() => _future = ApiService.instance.getPlayers());
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<List<PlayerInfo>>(
      future: _future,
      builder: (context, snapshot) {
        if (snapshot.connectionState == ConnectionState.waiting) {
          return const Center(child: CircularProgressIndicator());
        }

        if (snapshot.hasError) {
          return _FailureView(error: snapshot.error!, onRetry: _retry);
        }

        final players = snapshot.data ?? [];

        if (players.isEmpty) {
          return const Center(child: Text('Aucun joueur.'));
        }

        return ListView.builder(
          padding: const EdgeInsets.all(12),
          itemCount: players.length,
          itemBuilder: (context, index) {
            final player = players[index];

            return Card(
              child: ListTile(
                leading: CircleAvatar(child: Text('${player.number}')),
                title: Text(player.fullName),
                subtitle: Text(
                  '${player.teamName} · ${player.positionName}',
                ),
                trailing: const Icon(Icons.chevron_right),
                onTap: () {
                  Navigator.of(context).push(
                    MaterialPageRoute<void>(
                      builder: (_) => PlayerDetailsPage(id: player.id),
                    ),
                  );
                },
              ),
            );
          },
        );
      },
    );
  }
}

class TeamDetailsPage extends StatefulWidget {
  const TeamDetailsPage({super.key, required this.id});

  final int id;

  @override
  State<TeamDetailsPage> createState() => _TeamDetailsPageState();
}

class _TeamDetailsPageState extends State<TeamDetailsPage> {
  late Future<TeamDetails> _future;

  @override
  void initState() {
    super.initState();
    _future = ApiService.instance.getTeamDetails(widget.id);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Détail équipe')),
      body: FutureBuilder<TeamDetails>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return _FailureView(
              error: snapshot.error!,
              onRetry: () {
                setState(
                  () => _future =
                      ApiService.instance.getTeamDetails(widget.id),
                );
              },
            );
          }

          final team = snapshot.data;

          if (team == null) {
            return const Center(child: Text('Équipe introuvable.'));
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text(
                team.name,
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              Text('Ville : ${team.city}'),
              Text('Entraîneur : ${team.coachName}'),
              const SizedBox(height: 20),
              Text(
                'Joueurs (${team.players.length})',
                style: Theme.of(context).textTheme.titleLarge,
              ),
              if (team.players.isEmpty)
                const Padding(
                  padding: EdgeInsets.only(top: 12),
                  child: Text('Aucun joueur dans cette équipe.'),
                ),
              ...team.players.map(
                (player) => Card(
                  child: ListTile(
                    title: Text(player.fullName),
                    subtitle: Text(
                      '${player.positionName} · #${player.number}',
                    ),
                    trailing: const Icon(Icons.chevron_right),
                    onTap: () {
                      Navigator.of(context).push(
                        MaterialPageRoute<void>(
                          builder: (_) =>
                              PlayerDetailsPage(id: player.id),
                        ),
                      );
                    },
                  ),
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}

class PlayerDetailsPage extends StatefulWidget {
  const PlayerDetailsPage({super.key, required this.id});

  final int id;

  @override
  State<PlayerDetailsPage> createState() => _PlayerDetailsPageState();
}

class _PlayerDetailsPageState extends State<PlayerDetailsPage> {
  late Future<PlayerInfo> _future;

  @override
  void initState() {
    super.initState();
    _future = ApiService.instance.getPlayerDetails(widget.id);
  }

  Widget _stat(String label, int value) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 8),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('$label : $value / 100'),
          const SizedBox(height: 5),
          LinearProgressIndicator(value: value.clamp(0, 100) / 100),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Détail joueur')),
      body: FutureBuilder<PlayerInfo>(
        future: _future,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          }

          if (snapshot.hasError) {
            return _FailureView(
              error: snapshot.error!,
              onRetry: () {
                setState(
                  () => _future =
                      ApiService.instance.getPlayerDetails(widget.id),
                );
              },
            );
          }

          final player = snapshot.data;

          if (player == null) {
            return const Center(child: Text('Joueur introuvable.'));
          }

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              Text(
                player.fullName,
                style: Theme.of(context).textTheme.headlineSmall,
              ),
              Text('Âge : ${player.age} ans'),
              Text('Numéro : ${player.number}'),
              Text('Équipe : ${player.teamName}'),
              Text('Poste : ${player.positionName}'),
              Text(
                'État : ${player.isInjured ? 'Blessé' : 'Disponible'}',
              ),
              const SizedBox(height: 20),
              Text(
                'Note globale : ${player.overallRating}',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              _stat('Vitesse', player.speed),
              _stat('Endurance', player.endurance),
              _stat('Force', player.strength),
              _stat('Technique', player.technique),
            ],
          );
        },
      ),
    );
  }
}

class _FailureView extends StatelessWidget {
  const _FailureView({
    required this.error,
    required this.onRetry,
  });

  final Object error;
  final VoidCallback onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Text(
              'Impossible de contacter l’API.',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            Text(
              error.toString(),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 12),
            FilledButton(
              onPressed: onRetry,
              child: const Text('Réessayer'),
            ),
          ],
        ),
      ),
    );
  }
}