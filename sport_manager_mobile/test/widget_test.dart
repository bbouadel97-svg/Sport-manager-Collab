import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:sport_manager_mobile/main.dart';

void main() {
  test('Lecture des données d’une équipe', () {
    final team = TeamSummary.fromJson({
      'id': 1,
      'name': 'Eagles',
      'city': 'Testville',
      'coachName': 'Coach',
      'playerCount': 6,
    });

    expect(team.id, 1);
    expect(team.name, 'Eagles');
    expect(team.playerCount, 6);
    expect(team.city, 'Testville');
    expect(team.coachName, 'Coach');
  });

  testWidgets('Inscription refuse un formulaire vide', (tester) async {
    await tester.pumpWidget(
      MaterialApp(home: AuthPage(onAuthenticated: () {})),
    );

    await tester.tap(find.text('Créer un nouveau compte'));
    await tester.pumpAndSettle();
    await tester.tap(find.text('Créer mon compte'));
    await tester.pumpAndSettle();

    expect(find.text('Saisissez votre nom.'), findsOneWidget);
    expect(find.text('E-mail invalide.'), findsOneWidget);
    expect(find.text('8 caractères minimum.'), findsOneWidget);
  });
}