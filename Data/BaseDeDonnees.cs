using SQLite;

namespace PriceCheck.Data;

// Point d'accès unique à la base SQLite. Elle est créée une seule fois pour toute l'appli.
public class BaseDeDonnees
{
    // La connexion à la base (vide tant qu'on ne l'a pas ouverte)
    private SQLiteAsyncConnection? _connexion;

    // Verrou : empêche deux écrans d'initialiser la base en même temps
    private readonly SemaphoreSlim _verrou = new(1, 1);

    // Emplacement du fichier de base sur le téléphone, dans le dossier privé de l'appli
    public static string CheminFichier =>
        Path.Combine(FileSystem.AppDataDirectory, "pricecheck.db3");

    // Options d'ouverture : lecture + écriture, création du fichier s'il n'existe pas
    private const SQLiteOpenFlags Options =
        SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

    // Renvoie la connexion prête à l'emploi. La 1re fois, elle crée les tables et les données de départ.
    public async Task<SQLiteAsyncConnection> ObtenirConnexionAsync()
    {
        // Déjà prête : on la renvoie directement
        if (_connexion != null)
        {
            return _connexion;
        }

        await _verrou.WaitAsync();
        try
        {
            // On revérifie : un autre écran a peut-être fini l'initialisation pendant l'attente
            if (_connexion != null)
            {
                return _connexion;
            }

            var connexion = new SQLiteAsyncConnection(CheminFichier, Options);

            // Crée chaque table si elle n'existe pas encore (sans effet sinon)
            await connexion.CreateTableAsync<Produit>();
            await connexion.CreateTableAsync<Enseigne>();
            await connexion.CreateTableAsync<Releve>();
            await connexion.CreateTableAsync<Utilisateur>();

            await InsererDonneesInitialesAsync(connexion);

            _connexion = connexion;
            return _connexion;
        }
        finally
        {
            _verrou.Release(); // toujours libérer le verrou, même en cas d'erreur
        }
    }

    // Remplit la base avec les données de départ, SEULEMENT si elle est vide
    private static async Task InsererDonneesInitialesAsync(SQLiteAsyncConnection db)
    {
        if (await db.Table<Produit>().CountAsync() > 0)
        {
            return; // la base contient déjà des données : on ne touche à rien
        }

        // Transaction = "tout ou rien" : si l'appli plante au milieu, rien n'est enregistré à moitié
        await db.RunInTransactionAsync(conn =>
        {
            // 1) Les enseignes. Après Insert, l'objet reçoit son Id automatique.
            var superPlus = new Enseigne { Nom = "Super Plus" };
            var marche = new Enseigne { Nom = "Marché Central" };
            var miniPrix = new Enseigne { Nom = "Mini Prix" };
            var express = new Enseigne { Nom = "Express 24" };
            conn.Insert(superPlus);
            conn.Insert(marche);
            conn.Insert(miniPrix);
            conn.Insert(express);

            // 2) Les produits
            var lait = new Produit { CodeBarres = "6001234567890", Nom = "Lait entier 1 L" };
            var riz = new Produit { CodeBarres = "6001234567891", Nom = "Riz long grain 1 kg" };
            var huile = new Produit { CodeBarres = "6001234567892", Nom = "Huile de tournesol 1 L" };
            conn.Insert(lait);
            conn.Insert(riz);
            conn.Insert(huile);

            // 3) Les relevés de prix (produit, enseigne, prix en FCFA, date)
            var releves = new List<Releve>
            {
                // Lait : historique de Super Plus (identique à l'écran Historique)
                Nouveau(lait, superPlus, 1250, new DateTime(2026, 10, 5)),
                Nouveau(lait, superPlus, 1300, new DateTime(2026, 9, 21)),
                Nouveau(lait, superPlus, 1200, new DateTime(2026, 9, 7)),
                Nouveau(lait, superPlus, 1200, new DateTime(2026, 8, 24)),
                // Lait : autres enseignes (Express 24 a un relevé ancien de 3 semaines)
                Nouveau(lait, marche,   1400, new DateTime(2026, 10, 12)),
                Nouveau(lait, miniPrix, 1450, new DateTime(2026, 10, 14)),
                Nouveau(lait, express,  1600, new DateTime(2026, 10, 1)),

                // Riz
                Nouveau(riz, marche,   900,  new DateTime(2026, 10, 15)),
                Nouveau(riz, superPlus, 950, new DateTime(2026, 10, 15)),
                Nouveau(riz, miniPrix, 1000, new DateTime(2026, 10, 15)),
                Nouveau(riz, express,  1100, new DateTime(2026, 10, 15)),

                // Huile
                Nouveau(huile, superPlus, 1800, new DateTime(2026, 10, 16)),
                Nouveau(huile, marche,    1850, new DateTime(2026, 10, 16)),
                Nouveau(huile, miniPrix,  1900, new DateTime(2026, 10, 16)),
                Nouveau(huile, express,   2000, new DateTime(2026, 10, 16)),
            };
            conn.InsertAll(releves);
        });
    }

    // Petite méthode d'aide : construit un relevé à partir d'un produit, d'une enseigne, d'un prix et d'une date
    private static Releve Nouveau(Produit produit, Enseigne enseigne, int prix, DateTime date)
        => new Releve
        {
            ProduitId = produit.Id,
            EnseigneId = enseigne.Id,
            Prix = prix,
            DateReleve = date
        };
}