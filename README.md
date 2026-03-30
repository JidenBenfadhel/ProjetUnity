# 🚀 Projet Tank Arena

Bienvenue sur le repo GitHub de notre projet de développement Unity3D ! 
Ce fichier explique comment installer le projet en local sur vos machines et définit les règles strictes à suivre sur Git pour que l'on puisse travailler à 4 sans détruire le code des autres.

---

## 📥 1. Récupérer et lancer le projet (Première installation)

**Prérequis :** Avoir installé Unity Hub (avec la bonne version d'Unity) et Git.

Pour récupérer le projet sur ta machine :

1. Ouvre ton terminal (ou Git Bash) dans le dossier de ton choix.
2. Clone le dépôt distant avec cette commande :
   ```bash
   git clone https://github.com/JidenBenfadhel/ProjetUnity.git
   ```
3. Ouvre **Unity Hub**.
4. Clique sur **Open** (ou *Add project from disk*), navigue jusqu'au dossier cloné et sélectionne-le.
5. Lance le projet. Le premier chargement de l'éditeur peut prendre quelques minutes, c'est tout à fait normal.

---

## 🌿 2. Workflow Git : Nos règles d'équipe

Pour assurer une architecture propre et éviter les conflits infernaux, **il est formellement interdit de coder et de commit directement sur la branche `main`**. 

Voici la marche à suivre obligatoire pour développer une nouvelle fonctionnalité :

### Étape 1 : Mettre à jour sa base
Avant de commencer à coder, assure-toi d'être sur `main` et d'avoir la dernière version du jeu :
```bash
git checkout main
git pull origin main
```

### Étape 2 : Créer sa propre branche
Crée une nouvelle branche dédiée à ta tâche. Sois descriptif dans le nom (ex: `feat/mouvements-tank`, `fix/bug-menu`, `audio/musique`) :
```bash
git checkout -b nom-de-ta-branche
```

### Étape 3 : Travailler et Commit
Développe ta partie sur Unity. Sauvegarde régulièrement en faisant des commits avec des messages clairs :
```bash
git add .
git commit -m "Ajout de la mécanique de tir pour le joueur"
```

### Étape 4 : Envoyer sa branche sur GitHub
Quand ta fonctionnalité est terminée et qu'elle marche :
```bash
git push -u origin nom-de-ta-branche
```

### Étape 5 : Créer une Pull Request (PR)
1. Va sur la page GitHub du projet.
2. Une bannière te proposera de faire une **Compare & pull request**. Clique dessus.
3. Vérifie que tu envoies bien ta branche vers `main`.
4. Ajoute une courte description de ce que tu as codé.
5. Clique sur **Create pull request**.

⚠️ **Règle d'or : Ne valide jamais ta propre PR !** Demande toujours à un autre membre de l'équipe de relire ton code et de valider la fusion (Merge) vers `main`. Une fois fusionnée, tu pourras supprimer ta branche.
