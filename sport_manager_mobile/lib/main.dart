import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:http/http.dart' as http;

void main() {
  runApp(const SportManagerApp());
}

class ApiService {
  ApiService._();

  static final ApiService instance = ApiService._();
  static const FlutterSecureStorage _storage = FlutterSecureStorage();
  static const String _tokenKey = 'sport_manager_jwt';
  VoidCallback? onSessionExpired;

  static const String _configuredBaseUrl = String.fromEnvironment('API_BASE_URL');

  static String get _baseUrl {
    if (_configuredBaseUrl.isNotEmpty) return _configuredBaseUrl;
    if (!kIsWeb && defaultTargetPlatform == TargetPlatform.android) {
      return 'http://10.0.2.2:5084';
    }
    return 'http://localhost:5084';
  }

  Future<bool> hasToken() async => (await _storage.read(key: _tokenKey)) != null;

  Future<void> register({
    required String name,
    required String email,
    required String password,
  }) async {
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/auth/register'),
          headers: const {'Accept': 'application/json', 'Content-Type': 'application/json'},
          body: jsonEncode({'name': name, 'email': email, 'password': password}),
        )
        .timeout(const Duration(seconds: 10));

    if (response.statusCode != 201) {
      throw Exception(_errorMessage(response, 'Inscription impossible.'));
    }
  }

  Future<void> login({
    required String email,
    required String password,
  }) async {
    final response = await http
        .post(
          Uri.parse('$_baseUrl/api/auth/login'),
          headers: const {'Accept': 'application/json', 'Content-Type': 'application/json'},
          body: jsonEncode({'email': email, 'password': password}),
        )
        .timeout(const Duration(seconds: 10));

    if (response.statusCode != 200) {
      throw Exception(_errorMessage(response, 'Connexion impossible.'));
    }

    final data = _asMap(jsonDecode(response.body));
    final token = data['token'];
    if (token is! String || token.isEmpty) {
      throw const FormatException('Le serveur n’a pas renvoyé de jeton.');
    }
    await _storage.write(key: _tokenKey, value: token);
  }

  Future<void> logout() async {
    final token = await _storage.read(key: _tokenKey);
    try {
      if (token != null) {
        final response = await http
            .post(
              Uri.parse('$_baseUrl/api/auth/logout'),
              headers: {'Accept': 'application/json', 'Authorization': 'Bearer $token'},
            )
            .timeout(const Duration(seconds: 10));

        if (response.statusCode != 204 && response.statusCode != 401) {
          throw Exception(_errorMessage(response, 'La révocation du jeton a échoué.'));
        }
      }
    } finally {
      await _storage.delete(key: _tokenKey);
    }
  }

  Future<dynamic> _getJson(String path) async {
    final token = await _storage.read(key: _tokenKey);
    final response = await http
        .get(
          Uri.parse('$_baseUrl$path'),
          headers: {
            'Accept': 'application/json',
            if (token != null) 'Authorization': 'Bearer $token',
          },
        )
        .timeout(const Duration(seconds: 10));

    if (response.statusCode == 401) {
      await _storage.delete(key: _tokenKey);
      onSessionExpired?.call();
    }

    if (response.statusCode < 200 || response.statusCode >= 300) {
      throw Exception('Erreur HTTP ${response.statusCode} sur $path');
    }

    return jsonDecode(response.body);
  }

  String _errorMessage(http.Response response, String fallback) {
    try {
      final data = _asMap(jsonDecode(response.body));
      final message = data['message'];
      return message is String ? message : fallback;
    } on FormatException {
      return fallback;
    }
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
      home: const AuthGate(),
    );
  }
}

class AuthGate extends StatefulWidget {
  const AuthGate({super.key});

  @override
  State<AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<AuthGate> {
  bool? _authenticated;
  String? _restoreError;

  @override
  void initState() {
    super.initState();
    ApiService.instance.onSessionExpired = _expireSession;
    _restoreSession();
  }

  Future<void> _restoreSession() async {
    try {
      final hasToken = await ApiService.instance.hasToken();
      if (mounted) {
        setState(() {
          _authenticated = hasToken;
          _restoreError = null;
        });
      }
    } catch (error) {
      if (mounted) {
        setState(() => _restoreError = error.toString());
      }
    }
  }

  void _expireSession() {
    if (mounted) {
      setState(() => _authenticated = false);
    }
  }

  @override
  void dispose() {
    ApiService.instance.onSessionExpired = null;
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    if (_restoreError != null) {
      return Scaffold(
        body: Center(
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                Text('Impossible de restaurer la session : $_restoreError'),
                const SizedBox(height: 12),
                FilledButton(
                  onPressed: _restoreSession,
                  child: const Text('Réessayer'),
                ),
              ],
            ),
          ),
        ),
      );
    }
    if (_authenticated == null) {
      return const Scaffold(body: Center(child: CircularProgressIndicator()));
    }
    if (_authenticated!) {
      return HomePage(onLoggedOut: _expireSession);
    }
    return AuthPage(onAuthenticated: () {
      setState(() => _authenticated = true);
    });
  }
}

class AuthPage extends StatefulWidget {
  const AuthPage({super.key, required this.onAuthenticated});

  final VoidCallback onAuthenticated;

  @override
  State<AuthPage> createState() => _AuthPageState();
}

class _AuthPageState extends State<AuthPage> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _emailController = TextEditingController();
  final _passwordController = TextEditingController();
  bool _isRegister = false;
  bool _isBusy = false;
  String? _error;

  @override
  void dispose() {
    _nameController.dispose();
    _emailController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() {
      _isBusy = true;
      _error = null;
    });
    try {
      if (_isRegister) {
        await ApiService.instance.register(
          name: _nameController.text.trim(),
          email: _emailController.text.trim(),
          password: _passwordController.text,
        );
        if (mounted) setState(() => _isRegister = false);
      }
      await ApiService.instance.login(
        email: _emailController.text.trim(),
        password: _passwordController.text,
      );
      widget.onAuthenticated();
    } catch (error) {
      if (mounted) setState(() => _error = error.toString());
    } finally {
      if (mounted) setState(() => _isBusy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: Center(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(24),
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 420),
            child: Form(
              key: _formKey,
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  Text(
                    _isRegister ? 'Créer un compte' : 'Connexion',
                    style: Theme.of(context).textTheme.headlineMedium,
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 24),
                  if (_isRegister) ...[
                    TextFormField(
                      controller: _nameController,
                      decoration: const InputDecoration(labelText: 'Nom'),
                      textInputAction: TextInputAction.next,
                      validator: (value) => value == null || value.trim().isEmpty
                          ? 'Saisissez votre nom.'
                          : null,
                    ),
                    const SizedBox(height: 12),
                  ],
                  TextFormField(
                    controller: _emailController,
                    decoration: const InputDecoration(labelText: 'E-mail'),
                    keyboardType: TextInputType.emailAddress,
                    textInputAction: TextInputAction.next,
                    validator: (value) =>
                        value == null || !value.contains('@') ? 'E-mail invalide.' : null,
                  ),
                  const SizedBox(height: 12),
                  TextFormField(
                    controller: _passwordController,
                    decoration: const InputDecoration(labelText: 'Mot de passe'),
                    obscureText: true,
                    textInputAction: TextInputAction.done,
                    onFieldSubmitted: (_) => _submit(),
                    validator: (value) => value == null || value.length < 8
                        ? '8 caractères minimum.'
                        : null,
                  ),
                  if (_error != null) ...[
                    const SizedBox(height: 12),
                    Text(_error!, style: TextStyle(color: Theme.of(context).colorScheme.error)),
                  ],
                  const SizedBox(height: 20),
                  FilledButton(
                    onPressed: _isBusy ? null : _submit,
                    child: _isBusy
                        ? const CircularProgressIndicator()
                        : Text(_isRegister ? 'Créer mon compte' : 'Se connecter'),
                  ),
                  TextButton(
                    onPressed: _isBusy
                        ? null
                        : () => setState(() {
                              _isRegister = !_isRegister;
                              _error = null;
                            }),
                    child: Text(_isRegister
                        ? 'J’ai déjà un compte'
                        : 'Créer un nouveau compte'),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class HomePage extends StatefulWidget {
  const HomePage({super.key, required this.onLoggedOut});

  final VoidCallback onLoggedOut;

  @override
  State<HomePage> createState() => _HomePageState();
}

class _HomePageState extends State<HomePage> {
  int _selectedIndex = 0;
  bool _isLoggingOut = false;

  Future<void> _logout() async {
    setState(() => _isLoggingOut = true);
    try {
      await ApiService.instance.logout();
    } catch (error) {
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('Déconnexion locale effectuée, révocation impossible : $error')),
        );
      }
    } finally {
      widget.onLoggedOut();
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Text(_selectedIndex == 0 ? 'Équipes' : 'Joueurs'),
        actions: [
          IconButton(
            tooltip: 'Se déconnecter',
            onPressed: _isLoggingOut ? null : _logout,
            icon: const Icon(Icons.logout),
          ),
        ],
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
    setState(() {
      _future = ApiService.instance.getTeams();
    });
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
    setState(() {
      _future = ApiService.instance.getPlayers();
    });
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
                setState(() {
                  _future = ApiService.instance.getTeamDetails(widget.id);
                });
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
                setState(() {
                  _future = ApiService.instance.getPlayerDetails(widget.id);
                });
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