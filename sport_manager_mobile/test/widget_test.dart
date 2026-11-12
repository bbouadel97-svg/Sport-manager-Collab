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
}