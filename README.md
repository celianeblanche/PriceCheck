# PriceCheck

Application mobile (.NET MAUI, C#) qui compare le prix d'un produit emballé entre plusieurs enseignes à partir de son code-barres. Projet réalisé dans le cadre de l'Atelier de développement mobile.

## Fonctionnalités

- Inscription et connexion (compte enregistré dans la base, mot de passe haché avec PBKDF2 et un sel)
- Session mémorisée : l'appli s'ouvre directement sur l'accueil si on est déjà connecté, avec un lien de déconnexion
- Recherche d'un produit par son nom, sans scan
- Scan réel des codes-barres EAN-13 par la caméra (Google ML Kit), ou saisie manuelle
- Ajout d'un produit inconnu (nom, enseigne, prix) depuis le scan, la saisie manuelle ou l'accueil
- Comparaison des prix par enseigne, avec le meilleur prix mis en avant
- Signalement d'un prix (nouvelle enseigne possible), conservé après la fermeture de l'appli, avec une confirmation si le prix est très différent de celui connu
- Historique des prix du dernier produit consulté (minimum, moyen, maximum, variations)
- Liste d'achats enregistrée dans la base, propre à chaque compte, avec retrait d'un article
- Flèche « Retour » en haut des pages ; le bouton retour du téléphone repasse toujours par l'accueil avant de quitter l'appli

## Architecture

La couche données suit le principe vu en TP : les écrans demandent des **contrats** (interfaces), et l'implémentation SQLite est choisie dans `MauiProgram.cs`.

```
Controls/    Composant réutilisable (flèche « Retour »)
Data/        Entités SQLite, BaseDeDonnees, contrats (IProduitRepository, IUtilisateurRepository, IListeRepository) et leurs versions SQLite
Models/      Modèles d'affichage (lignes de prix, historique, articles de liste)
Services/    Formatage, hachage des mots de passe, session, dernier produit consulté, navigation retour, ajout de produit
Pages/       Écrans (XAML + code C#)
```

Base de données : SQLite (`sqlite-net-pcl`), fichier créé au premier lancement avec trois produits d'exemple (lait, riz, huile) et leurs prix.

## Lancer le projet

Prérequis : SDK .NET 10 avec la charge de travail MAUI, un téléphone Android (version 6.0 ou plus) en mode débogage USB.

```bash
dotnet build -f net10.0-android
```

## Tester

1. Ouvrir l'écran **Inscription** et créer un compte (n'importe quel e-mail, mot de passe de 8 caractères minimum).
2. Sur l'Accueil, taper `lait` dans la recherche, puis ouvrir la comparaison.
3. **Signaler un prix** et constater qu'il reste après la fermeture de l'appli.
4. Onglet **Scanner** : scanner un vrai produit emballé. S'il est inconnu, l'appli propose de l'ajouter ; le scanner de nouveau ouvre sa comparaison.

Codes-barres d'exemple pour la saisie manuelle : `6001234567890` (lait), `6001234567891` (riz), `6001234567892` (huile). Ce ne sont pas de vrais EAN-13, donc le scan caméra ne peut pas les lire.

## Limites connues

- La base est locale à l'appareil : un produit ajouté sur un téléphone n'apparaît pas sur un autre.
- Seuls les codes-barres EAN-13 (13 chiffres) sont lus ; les codes de 8 ou 12 chiffres ne le sont pas.
- La fonction « Mot de passe oublié » n'est pas disponible.
- Testé sur Android uniquement.