# PriceCheck

Application mobile .NET MAUI (C#) pour comparer le prix d'un produit emballé entre plusieurs enseignes à partir de son code-barres.

**État actuel : front-end uniquement.** Le backend, la base de données et les données réelles sont prévus dans une phase suivante.

## Écrans

1. Connexion
2. Inscription
3. Accueil
4. Scanner (saisie du code-barres)
5. Résultat (prix par enseigne)
6. Ajout d'un relevé
7. Ma liste
8. Historique des prix

## Ce qui est simulé

- Les produits, les prix, les enseignes et l'historique sont des données fictives écrites dans le code (dossier `Services`).
- La connexion et l'inscription ne font aucune authentification réelle : aucun compte n'est créé sur un serveur.
- Le scan par caméra n'est pas implémenté : toucher le cadre de la caméra affiche un message et utilise un code-barres de démonstration, sinon le code-barres se saisit manuellement.
- Aucun backend, aucune API, aucune base de données.

## Organisation de l'historique Git

Les phases ci-dessous sont un découpage choisi par l'auteure, par parcours d'utilisation :

- **Initialisation** : structure du projet, styles, navigation.
- **Phase A** : connexion et inscription.
- **Phase B** : accueil, scanner et résultat.
- **Phase C** : ajout d'un relevé, ma liste et historique.

Chaque commit de phase ajoute les pages concernées ; seul l'état final du dépôt compile, car la navigation (`AppShell`) référence toutes les pages.

## Lancer le projet

Prérequis : SDK .NET avec la charge de travail MAUI, Android SDK et JDK.

    dotnet build -f net10.0-android

Dossiers principaux : `Pages` (écrans XAML et C#), `Models` (modèles de données), `Services` (données simulées), `Resources/Styles` (couleurs et styles).
