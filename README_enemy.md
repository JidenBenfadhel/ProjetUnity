# 📝 Documentation IA : Système d'Ennemis (Wii Play Tanks)

Ce document explique comment utiliser et configurer le nouveau script `EnemyController` pour créer des situations de combat variées. Le système utilise une architecture par "États" (Enums) et s'appuie sur le **NavMesh** d'Unity pour les déplacements.

## ⚠️ Prérequis (Très Important)
Pour que les ennemis fonctionnent correctement dans une scène, le Level Designer doit impérativement s'assurer de ces trois points :
1. **NavMesh Surface :** La scène doit posséder un objet avec le composant `NavMesh Surface` et la zone bleue (Bake) doit être générée.
2. **Layer des Tanks :** Les tanks (Joueur et Ennemis) doivent être sur le calque `Tanks`, et ce calque doit être **décoché** dans les paramètres *Include Layers* du NavMesh Surface.
3. **Le Tag du Joueur :** Le tank du joueur doit impérativement posséder le tag `Player`, sinon les ennemis ne le verront pas.

---

## ⚙️ Configuration dans l'Inspector

Glissez simplement le Prefab `EnemyTankV2` dans la scène. Dans le composant `Enemy Controller`, vous trouverez plusieurs catégories pour personnaliser son comportement.

### 1. Configuration IA (Les Profils)
C'est ici que l'on définit la "personnalité" et la dangerosité du tank.

*   **Enemy Type :**
    *   `Rusher` : Fonceur. Il cherche à réduire la distance le plus vite possible.
    *   `Tactical` : Défensif. Il scanne l'environnement pour trouver le mur le plus proche et se cacher derrière, tout en gardant une ligne de vue si possible.
    *   `Sniper` : Fuyard/Distance. Il maintient une distance de sécurité stricte. Si le joueur s'approche trop, il fait marche arrière pour fuir.
*   **Difficulty Level :**
    *   `Level 1` : Tir imprécis (grosse dispersion), facile à esquiver.
    *   `Level 2` : Légère imprécision, demande au joueur de rester mobile.
    *   `Level 3` : Précision parfaite (calcule les angles) **ET** cadence de tir doublée. En mode Sniper, le niveau 3 lui permet de calculer les rebonds sur les murs !

### 2. Statistiques de base
À ajuster selon le type d'ennemi choisi :
*   **Move Speed :** Vitesse de déplacement du tank. *(Conseil : Un Rusher devrait être à 4 ou 5, un Sniper à 3).*
*   **Stop Distance :** La distance à laquelle le tank s'arrête d'avancer vers le joueur. *(Conseil : ~2.5 pour un Rusher, > 12 pour un Sniper).*
*   **Fire Rate :** Temps (en secondes) entre chaque tir.

### 3. Paramètres Tactiques (Uniquement pour le type 'Tactical')
*   **Search Cover Radius :** La zone de scan (en mètres) dans laquelle il cherche un mur pour se cacher.
*   **Cover Offset :** La distance derrière le mur où il va se garer. Augmentez cette valeur s'il se cogne trop contre son abri.

---

## 🧠 Comportements Attendus (Expected Behaviors)

*   **Anti-Suicide :** Tous les ennemis possèdent un laser de détection. Si un mur se trouve directement devant leur canon, ils bloquent leur tir pour ne pas détruire leur propre obus ou se faire exploser par un rebond immédiat. Ils tireront à la seconde où la voie sera libre.
*   **Stabilité Physique :** Le `Rigidbody` des ennemis est paramétré sur **Is Kinematic**. Ils ne peuvent pas être renversés ou poussés par la physique du jeu, garantissant que leur pathfinding ne "casse" jamais.
*   **Le Radar du Sniper :** Le Sniper est l'ennemi le plus avancé. Il tire 360 rayons invisibles par frame pour trouver le meilleur angle d'attaque. S'il ne peut pas vous toucher en ligne droite, il cherchera un mur pour effectuer un tir à rebond mathématiquement parfait. *(Attention : le prefab du projectile ennemi doit autoriser au moins 1 rebond).*

## 🛠️ Dépannage rapide (Troubleshooting)

| Problème rencontré | Solution possible |
| :--- | :--- |
| **L'ennemi ne bouge pas du tout** | Vérifier que l'arène a bien été *Bake* (NavMesh) et que le char est posé sur la zone bleue. |
| **L'ennemi ne tire plus jamais** | Vérifier que le sol n'a pas reçu le Tag `Wall` par erreur, ou que le `FirePoint` n'est pas coincé à l'intérieur du canon. |
| **Le Sniper refuse de reculer** | Assurez-vous que la `Stop Distance` est suffisamment grande (ex: 15). Si elle est trop basse, la zone de fuite calculée se trouve au même endroit que lui. |
| **L'ennemi fait la toupie / tourne très vite** | Vérifier que le `Rigidbody` est bien sur *Is Kinematic* et que l' *Angular Damping* est à 0. |