using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProlabWeb.Db;

public partial class ProlabwebContext : DbContext
{
    public ProlabwebContext()
    {
    }

    public ProlabwebContext(DbContextOptions<ProlabwebContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Affectationcaisse> Affectationcaisses { get; set; }

    public virtual DbSet<Analyse> Analyses { get; set; }

    public virtual DbSet<Analysemateriel> Analysemateriels { get; set; }

    public virtual DbSet<Assurance> Assurances { get; set; }

    public virtual DbSet<Automate> Automates { get; set; }

    public virtual DbSet<Caisse> Caisses { get; set; }

    public virtual DbSet<Caisseappro> Caisseappros { get; set; }

    public virtual DbSet<Caissedepense> Caissedepenses { get; set; }

    public virtual DbSet<Categorie> Categories { get; set; }

    public virtual DbSet<Categorieanalyse> Categorieanalyses { get; set; }

    public virtual DbSet<Categoriemateriel> Categoriemateriels { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Demandeanalysemateriel> Demandeanalysemateriels { get; set; }

    public virtual DbSet<Detaildemande> Detaildemandes { get; set; }

    public virtual DbSet<Detailfacture> Detailfactures { get; set; }

    public virtual DbSet<Detailmaterielappro> Detailmaterielappros { get; set; }

    public virtual DbSet<Detailresultat> Detailresultats { get; set; }

    public virtual DbSet<Entetedemande> Entetedemandes { get; set; }

    public virtual DbSet<Entetefacture> Entetefactures { get; set; }

    public virtual DbSet<Entetematerielappro> Entetematerielappros { get; set; }

    public virtual DbSet<Enteteresultat> Enteteresultats { get; set; }

    public virtual DbSet<Fournisseur> Fournisseurs { get; set; }

    public virtual DbSet<Loboratoire> Loboratoires { get; set; }

    public virtual DbSet<Materiel> Materiels { get; set; }

    public virtual DbSet<Methode> Methodes { get; set; }

    public virtual DbSet<Methodeanalyse> Methodeanalyses { get; set; }

    public virtual DbSet<Natureechantillon> Natureechantillons { get; set; }

    public virtual DbSet<Parametre> Parametres { get; set; }

    public virtual DbSet<Partenaire> Partenaires { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Photopatient> Photopatients { get; set; }

    public virtual DbSet<Policeassurance> Policeassurances { get; set; }

    public virtual DbSet<Prelevement> Prelevements { get; set; }

    public virtual DbSet<Preleveur> Preleveurs { get; set; }

    public virtual DbSet<Prescripteur> Prescripteurs { get; set; }

    public virtual DbSet<Profil> Profils { get; set; }

    public virtual DbSet<Sexe> Sexes { get; set; }

    public virtual DbSet<Sexeautorise> Sexeautorises { get; set; }

    public virtual DbSet<Signatureutilisateur> Signatureutilisateurs { get; set; }

    public virtual DbSet<Site> Sites { get; set; }

    public virtual DbSet<Tarifanalyseassurance> Tarifanalyseassurances { get; set; }

    public virtual DbSet<Tarifcategorieassurance> Tarifcategorieassurances { get; set; }

    public virtual DbSet<Typeassurance> Typeassurances { get; set; }

    public virtual DbSet<Typedocumentidentite> Typedocumentidentites { get; set; }

    public virtual DbSet<Typepeau> Typepeaus { get; set; }

    public virtual DbSet<Unite> Unites { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    public virtual DbSet<Utilisateurlaboratoire> Utilisateurlaboratoires { get; set; }

    public virtual DbSet<Utilisateurprofil> Utilisateurprofils { get; set; }

    public virtual DbSet<Valeurreference> Valeurreferences { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=65.21.245.245;Database=prolabweb;Username=testuser;Password=2025M@dbUsr#@!");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Affectationcaisse>(entity =>
        {
            entity.HasKey(e => e.Affectationcaisseid).HasName("affectationcaisse_pk");

            entity.ToTable("affectationcaisse");

            entity.Property(e => e.Affectationcaisseid)
                .ValueGeneratedNever()
                .HasColumnName("affectationcaisseid");
            entity.Property(e => e.Caisseid).HasColumnName("caisseid");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.Caisse).WithMany(p => p.Affectationcaisses)
                .HasForeignKey(d => d.Caisseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("affectationcaisse_caisse_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Affectationcaisses)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("affectationcaisse_utilisateur_fk");
        });

        modelBuilder.Entity<Analyse>(entity =>
        {
            entity.HasKey(e => e.Idanalyse).HasName("analyse_pk");

            entity.ToTable("analyse");

            entity.HasIndex(e => e.Nom, "analyse_unique_nom").IsUnique();

            entity.Property(e => e.Idanalyse)
                .ValueGeneratedNever()
                .HasColumnName("idanalyse");
            entity.Property(e => e.Accredite)
                .HasDefaultValue(false)
                .HasColumnName("accredite");
            entity.Property(e => e.Affichermachineresultat)
                .HasDefaultValue(false)
                .HasColumnName("affichermachineresultat");
            entity.Property(e => e.Affichermethodresultat)
                .HasDefaultValue(false)
                .HasColumnName("affichermethodresultat");
            entity.Property(e => e.Aliascodeautomate)
                .HasMaxLength(256)
                .HasColumnName("aliascodeautomate");
            entity.Property(e => e.Avecautomate)
                .HasDefaultValue(false)
                .HasColumnName("avecautomate");
            entity.Property(e => e.Codeparametre)
                .HasMaxLength(16)
                .HasColumnName("codeparametre");
            entity.Property(e => e.Codeunite)
                .HasMaxLength(8)
                .HasColumnName("codeunite");
            entity.Property(e => e.Codeunitesi)
                .HasMaxLength(8)
                .HasColumnName("codeunitesi");
            entity.Property(e => e.Codification)
                .HasMaxLength(256)
                .HasColumnName("codification");
            entity.Property(e => e.Commentaire).HasColumnName("commentaire");
            entity.Property(e => e.Decimalresultatsi).HasColumnName("decimalresultatsi");
            entity.Property(e => e.Decimalresultatstandard).HasColumnName("decimalresultatstandard");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Facteurconversionsi)
                .HasPrecision(18, 2)
                .HasColumnName("facteurconversionsi");
            entity.Property(e => e.Formuleautomate)
                .HasMaxLength(256)
                .HasColumnName("formuleautomate");
            entity.Property(e => e.Idanalyseparent).HasColumnName("idanalyseparent");
            entity.Property(e => e.Idautomate).HasColumnName("idautomate");
            entity.Property(e => e.Idlaboratoire)
                .ValueGeneratedOnAdd()
                .HasColumnName("idlaboratoire");
            entity.Property(e => e.Idnatureechantillon).HasColumnName("idnatureechantillon");
            entity.Property(e => e.Indicederev)
                .HasMaxLength(16)
                .HasColumnName("indicederev");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Ordreaffichage)
                .HasDefaultValue(0)
                .HasColumnName("ordreaffichage");
            entity.Property(e => e.Prix)
                .HasPrecision(18, 2)
                .HasColumnName("prix");

            entity.HasOne(d => d.CodeuniteNavigation).WithMany(p => p.AnalyseCodeuniteNavigations)
                .HasForeignKey(d => d.Codeunite)
                .HasConstraintName("analyse_unite_fk");

            entity.HasOne(d => d.CodeunitesiNavigation).WithMany(p => p.AnalyseCodeunitesiNavigations)
                .HasForeignKey(d => d.Codeunitesi)
                .HasConstraintName("analyse_unite_fk_unitesi");

            entity.HasOne(d => d.IdanalyseparentNavigation).WithMany(p => p.InverseIdanalyseparentNavigation)
                .HasForeignKey(d => d.Idanalyseparent)
                .HasConstraintName("analyse_analyse_fk");

            entity.HasOne(d => d.IdautomateNavigation).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.Idautomate)
                .HasConstraintName("analyse_automate_fk_automate");

            entity.HasOne(d => d.IdlaboratoireNavigation).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.Idlaboratoire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("analyse_loboratoire_fk");

            entity.HasOne(d => d.IdnatureechantillonNavigation).WithMany(p => p.Analyses)
                .HasForeignKey(d => d.Idnatureechantillon)
                .HasConstraintName("analyse_natureechantillon_fk");
        });

        modelBuilder.Entity<Analysemateriel>(entity =>
        {
            entity.HasKey(e => e.Analysematerielid).HasName("analysemateriel_pk");

            entity.ToTable("analysemateriel");

            entity.Property(e => e.Analysematerielid)
                .ValueGeneratedNever()
                .HasColumnName("analysematerielid");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Materielid).HasColumnName("materielid");
            entity.Property(e => e.Quantite)
                .HasPrecision(20, 2)
                .HasColumnName("quantite");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Analysemateriels)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("analysemateriel_analyse_fk");

            entity.HasOne(d => d.Materiel).WithMany(p => p.Analysemateriels)
                .HasForeignKey(d => d.Materielid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("analysemateriel_materiel_fk");
        });

        modelBuilder.Entity<Assurance>(entity =>
        {
            entity.HasKey(e => e.Codeassurance).HasName("assurance_pk");

            entity.ToTable("assurance");

            entity.HasIndex(e => e.Nom, "assurance_uknom").IsUnique();

            entity.Property(e => e.Codeassurance)
                .HasMaxLength(16)
                .HasColumnName("codeassurance");
            entity.Property(e => e.Codetypeassurance)
                .HasMaxLength(8)
                .HasColumnName("codetypeassurance");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");

            entity.HasOne(d => d.CodetypeassuranceNavigation).WithMany(p => p.Assurances)
                .HasForeignKey(d => d.Codetypeassurance)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("assurance_typeassurance_fk");
        });

        modelBuilder.Entity<Automate>(entity =>
        {
            entity.HasKey(e => e.Idautomate).HasName("automate_pk");

            entity.ToTable("automate");

            entity.Property(e => e.Idautomate).HasColumnName("idautomate");
            entity.Property(e => e.Confentree)
                .HasMaxLength(32)
                .HasColumnName("confentree");
            entity.Property(e => e.Confsortie)
                .HasMaxLength(32)
                .HasColumnName("confsortie");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Modeauto)
                .HasMaxLength(32)
                .HasColumnName("modeauto");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.Parserconfig)
                .HasMaxLength(32)
                .HasColumnName("parserconfig");
            entity.Property(e => e.Processconfig)
                .HasMaxLength(32)
                .HasColumnName("processconfig");
        });

        modelBuilder.Entity<Caisse>(entity =>
        {
            entity.HasKey(e => e.Caisseid).HasName("caisse_pk");

            entity.ToTable("caisse");

            entity.Property(e => e.Caisseid)
                .ValueGeneratedNever()
                .HasColumnName("caisseid");
            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");

            entity.HasOne(d => d.CodesiteNavigation).WithMany(p => p.Caisses)
                .HasForeignKey(d => d.Codesite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caisse_site_fk");
        });

        modelBuilder.Entity<Caisseappro>(entity =>
        {
            entity.HasKey(e => e.Caisseapproid).HasName("caisseappro_pk");

            entity.ToTable("caisseappro");

            entity.Property(e => e.Caisseapproid)
                .ValueGeneratedNever()
                .HasColumnName("caisseapproid");
            entity.Property(e => e.Caisseid).HasColumnName("caisseid");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Montant)
                .HasPrecision(20, 2)
                .HasColumnName("montant");

            entity.HasOne(d => d.Caisse).WithMany(p => p.Caisseappros)
                .HasForeignKey(d => d.Caisseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caisseappro_caisse_fk");
        });

        modelBuilder.Entity<Caissedepense>(entity =>
        {
            entity.HasKey(e => e.Caissedepenseid).HasName("caissetransaction_pk");

            entity.ToTable("caissedepense");

            entity.Property(e => e.Caissedepenseid)
                .ValueGeneratedNever()
                .HasColumnName("caissedepenseid");
            entity.Property(e => e.Caisseid).HasColumnName("caisseid");
            entity.Property(e => e.Montant)
                .HasPrecision(20, 2)
                .HasColumnName("montant");
            entity.Property(e => e.Motif).HasColumnName("motif");

            entity.HasOne(d => d.Caisse).WithMany(p => p.Caissedepenses)
                .HasForeignKey(d => d.Caisseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("caissedepense_caisse_fk");
        });

        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.HasKey(e => e.Categorieid).HasName("categorie_pk");

            entity.ToTable("categorie");

            entity.HasIndex(e => e.Nom, "categorie_uk_name").IsUnique();

            entity.Property(e => e.Categorieid)
                .ValueGeneratedNever()
                .HasColumnName("categorieid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Prix)
                .HasPrecision(18, 2)
                .HasColumnName("prix");
        });

        modelBuilder.Entity<Categorieanalyse>(entity =>
        {
            entity.HasKey(e => e.Categorieanalyseid).HasName("categorieanalyse_pk");

            entity.ToTable("categorieanalyse");

            entity.HasIndex(e => new { e.Categorieid, e.Idanalyse }, "categorieanalyse_uk_cat_analyse").IsUnique();

            entity.Property(e => e.Categorieanalyseid)
                .ValueGeneratedNever()
                .HasColumnName("categorieanalyseid");
            entity.Property(e => e.Categorieid).HasColumnName("categorieid");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");

            entity.HasOne(d => d.Categorie).WithMany(p => p.Categorieanalyses)
                .HasForeignKey(d => d.Categorieid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("categorieanalyse_categorie_fk");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Categorieanalyses)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("categorieanalyse_analyse_fk");
        });

        modelBuilder.Entity<Categoriemateriel>(entity =>
        {
            entity.HasKey(e => e.Categoriematerielid).HasName("categoriemateriel_pk");

            entity.ToTable("categoriemateriel");

            entity.HasIndex(e => e.Nom, "categoriemateriel_unique").IsUnique();

            entity.Property(e => e.Categoriematerielid)
                .ValueGeneratedNever()
                .HasColumnName("categoriematerielid");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Countryisocode2).HasName("country_pk");

            entity.ToTable("country");

            entity.HasIndex(e => e.Countryisocode3, "country_un").IsUnique();

            entity.HasIndex(e => e.Internetdomain, "country_un_internetdom").IsUnique();

            entity.HasIndex(e => e.Name, "country_un_name").IsUnique();

            entity.Property(e => e.Countryisocode2)
                .HasMaxLength(2)
                .HasColumnName("countryisocode2");
            entity.Property(e => e.Countryisocode3)
                .HasMaxLength(3)
                .HasColumnName("countryisocode3");
            entity.Property(e => e.Currencycode)
                .HasMaxLength(8)
                .HasColumnName("currencycode");
            entity.Property(e => e.Internetdomain)
                .HasMaxLength(16)
                .HasColumnName("internetdomain");
            entity.Property(e => e.Name)
                .HasMaxLength(128)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Demandeanalysemateriel>(entity =>
        {
            entity.HasKey(e => e.Demandeanalysematerielid).HasName("demandeanalysemateriel_pk");

            entity.ToTable("demandeanalysemateriel");

            entity.Property(e => e.Demandeanalysematerielid)
                .ValueGeneratedNever()
                .HasColumnName("demandeanalysematerielid");
            entity.Property(e => e.Analysematerielid).HasColumnName("analysematerielid");
            entity.Property(e => e.Entetedemandeid).HasColumnName("entetedemandeid");
            entity.Property(e => e.Quantite)
                .HasPrecision(20, 2)
                .HasColumnName("quantite");

            entity.HasOne(d => d.Analysemateriel).WithMany(p => p.Demandeanalysemateriels)
                .HasForeignKey(d => d.Analysematerielid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("demandeanalysemateriel_analysemateriel_fk");

            entity.HasOne(d => d.Entetedemande).WithMany(p => p.Demandeanalysemateriels)
                .HasForeignKey(d => d.Entetedemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("demandeanalysemateriel_entetedemande_fk");
        });

        modelBuilder.Entity<Detaildemande>(entity =>
        {
            entity.HasKey(e => e.Detaildemandeid).HasName("detaildemande_pk");

            entity.ToTable("detaildemande");

            entity.HasIndex(e => new { e.Idanalyse, e.Entetedemandeid }, "detaildemande_unique").IsUnique();

            entity.Property(e => e.Detaildemandeid)
                .ValueGeneratedNever()
                .HasColumnName("detaildemandeid");
            entity.Property(e => e.Categorieid).HasColumnName("categorieid");
            entity.Property(e => e.Complement)
                .HasPrecision(20, 2)
                .HasColumnName("complement");
            entity.Property(e => e.Entetedemandeid).HasColumnName("entetedemandeid");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Net)
                .HasPrecision(20, 2)
                .HasColumnName("net");
            entity.Property(e => e.Partassurance)
                .HasPrecision(20, 2)
                .HasColumnName("partassurance");
            entity.Property(e => e.Partpatient)
                .HasPrecision(20, 2)
                .HasColumnName("partpatient");
            entity.Property(e => e.Prix)
                .HasPrecision(18, 2)
                .HasColumnName("prix");

            entity.HasOne(d => d.Categorie).WithMany(p => p.Detaildemandes)
                .HasForeignKey(d => d.Categorieid)
                .HasConstraintName("detaildemande_categorie_fk");

            entity.HasOne(d => d.Entetedemande).WithMany(p => p.Detaildemandes)
                .HasForeignKey(d => d.Entetedemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detaildemande_entetedemande_fk");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Detaildemandes)
                .HasForeignKey(d => d.Idanalyse)
                .HasConstraintName("detaildemande_analyse_fk");
        });

        modelBuilder.Entity<Detailfacture>(entity =>
        {
            entity.HasKey(e => e.Detailfactureid).HasName("detailfacture_pk");

            entity.ToTable("detailfacture");

            entity.Property(e => e.Detailfactureid)
                .ValueGeneratedNever()
                .HasColumnName("detailfactureid");
            entity.Property(e => e.Detaildemandeid).HasColumnName("detaildemandeid");
            entity.Property(e => e.Entetefactureid).HasColumnName("entetefactureid");

            entity.HasOne(d => d.Detaildemande).WithMany(p => p.Detailfactures)
                .HasForeignKey(d => d.Detaildemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailfacture_detaildemande_fk");

            entity.HasOne(d => d.Entetefacture).WithMany(p => p.Detailfactures)
                .HasForeignKey(d => d.Entetefactureid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailfacture_entetefacture_fk");
        });

        modelBuilder.Entity<Detailmaterielappro>(entity =>
        {
            entity.HasKey(e => e.Detailmaterielapproid).HasName("detailmaterielappro_pk");

            entity.ToTable("detailmaterielappro");

            entity.Property(e => e.Detailmaterielapproid)
                .ValueGeneratedNever()
                .HasColumnName("detailmaterielapproid");
            entity.Property(e => e.Entetematerielapproid).HasColumnName("entetematerielapproid");
            entity.Property(e => e.Materielid).HasColumnName("materielid");
            entity.Property(e => e.Quantite)
                .HasPrecision(20, 2)
                .HasColumnName("quantite");

            entity.HasOne(d => d.Entetematerielappro).WithMany(p => p.Detailmaterielappros)
                .HasForeignKey(d => d.Entetematerielapproid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailmaterielappro_entetematerielappro_fk");

            entity.HasOne(d => d.Materiel).WithMany(p => p.Detailmaterielappros)
                .HasForeignKey(d => d.Materielid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailmaterielappro_materiel_fk");
        });

        modelBuilder.Entity<Detailresultat>(entity =>
        {
            entity.HasKey(e => e.Detailresultatid).HasName("detailresultat_pk");

            entity.ToTable("detailresultat");

            entity.Property(e => e.Detailresultatid)
                .ValueGeneratedNever()
                .HasColumnName("detailresultatid");
            entity.Property(e => e.Commentaire).HasColumnName("commentaire");
            entity.Property(e => e.Databuilder).HasColumnName("databuilder");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Enteteresultatid).HasColumnName("enteteresultatid");
            entity.Property(e => e.Parametreid).HasColumnName("parametreid");
            entity.Property(e => e.Resultat)
                .HasColumnType("character varying")
                .HasColumnName("resultat");
            entity.Property(e => e.Resultatsi)
                .HasColumnType("character varying")
                .HasColumnName("resultatsi");

            entity.HasOne(d => d.Enteteresultat).WithMany(p => p.Detailresultats)
                .HasForeignKey(d => d.Enteteresultatid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailresultat_enteteresultat_fk");

            entity.HasOne(d => d.Parametre).WithMany(p => p.Detailresultats)
                .HasForeignKey(d => d.Parametreid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("detailresultat_parametre_fk");
        });

        modelBuilder.Entity<Entetedemande>(entity =>
        {
            entity.HasKey(e => e.Entetedemandeid).HasName("entetedemande_pk");

            entity.ToTable("entetedemande");

            entity.HasIndex(e => e.Numero, "entetedemande_unique").IsUnique();

            entity.Property(e => e.Entetedemandeid)
                .ValueGeneratedNever()
                .HasColumnName("entetedemandeid");
            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Numero)
                .HasMaxLength(128)
                .HasColumnName("numero");
            entity.Property(e => e.Ordre)
                .ValueGeneratedOnAdd()
                .HasColumnName("ordre");
            entity.Property(e => e.Partenaireid).HasColumnName("partenaireid");
            entity.Property(e => e.Patientid).HasColumnName("patientid");
            entity.Property(e => e.Policeassuranceid).HasColumnName("policeassuranceid");
            entity.Property(e => e.Prescripteurid).HasColumnName("prescripteurid");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.CodesiteNavigation).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Codesite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetedemande_site_fk");

            entity.HasOne(d => d.Partenaire).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Partenaireid)
                .HasConstraintName("entetedemande_partenaire_fk");

            entity.HasOne(d => d.Patient).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Patientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetedemande_patient_fk");

            entity.HasOne(d => d.Policeassurance).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Policeassuranceid)
                .HasConstraintName("entetedemande_policeassurance_fk");

            entity.HasOne(d => d.Prescripteur).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Prescripteurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetedemande_prescripteur_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Entetedemandes)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetedemande_utilisateur_fk");
        });

        modelBuilder.Entity<Entetefacture>(entity =>
        {
            entity.HasKey(e => e.Entetefactureid).HasName("entetefacture_pk");

            entity.ToTable("entetefacture");

            entity.Property(e => e.Entetefactureid)
                .ValueGeneratedNever()
                .HasColumnName("entetefactureid");
            entity.Property(e => e.Caisseid).HasColumnName("caisseid");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Entetedemandeid).HasColumnName("entetedemandeid");
            entity.Property(e => e.Numero)
                .HasMaxLength(128)
                .HasColumnName("numero");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.Caisse).WithMany(p => p.Entetefactures)
                .HasForeignKey(d => d.Caisseid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetefacture_caisse_fk");

            entity.HasOne(d => d.Entetedemande).WithMany(p => p.Entetefactures)
                .HasForeignKey(d => d.Entetedemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetefacture_entetedemande_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Entetefactures)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetefacture_utilisateur_fk");
        });

        modelBuilder.Entity<Entetematerielappro>(entity =>
        {
            entity.HasKey(e => e.Entetematerielapproid).HasName("entetematerielappro_pk");

            entity.ToTable("entetematerielappro");

            entity.HasIndex(e => e.Numero, "entetematerielappro_unique").IsUnique();

            entity.Property(e => e.Entetematerielapproid)
                .ValueGeneratedNever()
                .HasColumnName("entetematerielapproid");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Fournisseurid).HasColumnName("fournisseurid");
            entity.Property(e => e.Numero)
                .HasMaxLength(256)
                .HasColumnName("numero");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.Fournisseur).WithMany(p => p.Entetematerielappros)
                .HasForeignKey(d => d.Fournisseurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetematerielappro_fournisseur_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Entetematerielappros)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("entetematerielappro_utilisateur_fk");
        });

        modelBuilder.Entity<Enteteresultat>(entity =>
        {
            entity.HasKey(e => e.Enteteresultatid).HasName("enteteresultat_pk");

            entity.ToTable("enteteresultat");

            entity.Property(e => e.Enteteresultatid)
                .ValueGeneratedNever()
                .HasColumnName("enteteresultatid");
            entity.Property(e => e.Biologisteid).HasColumnName("biologisteid");
            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Date)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date");
            entity.Property(e => e.Entetedemandeid).HasColumnName("entetedemandeid");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Interpretation).HasColumnName("interpretation");
            entity.Property(e => e.Statut)
                .HasMaxLength(32)
                .HasColumnName("statut");
            entity.Property(e => e.Technicienid).HasColumnName("technicienid");
            entity.Property(e => e.Validationbiologiste).HasColumnName("validationbiologiste");
            entity.Property(e => e.Validationtechnicien)
                .HasDefaultValue(false)
                .HasColumnName("validationtechnicien");

            entity.HasOne(d => d.Biologiste).WithMany(p => p.EnteteresultatBiologistes)
                .HasForeignKey(d => d.Biologisteid)
                .HasConstraintName("enteteresultat_utilisateur_fk_1");

            entity.HasOne(d => d.CodesiteNavigation).WithMany(p => p.Enteteresultats)
                .HasForeignKey(d => d.Codesite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enteteresultat_site_fk");

            entity.HasOne(d => d.Entetedemande).WithMany(p => p.Enteteresultats)
                .HasForeignKey(d => d.Entetedemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enteteresultat_entetedemande_fk");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Enteteresultats)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enteteresultat_analyse_fk");

            entity.HasOne(d => d.Technicien).WithMany(p => p.EnteteresultatTechniciens)
                .HasForeignKey(d => d.Technicienid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("enteteresultat_utilisateur_fk");
        });

        modelBuilder.Entity<Fournisseur>(entity =>
        {
            entity.HasKey(e => e.Fournisseurid).HasName("fournisseur_pk");

            entity.ToTable("fournisseur");

            entity.HasIndex(e => e.Nom, "fournisseur_unique").IsUnique();

            entity.Property(e => e.Fournisseurid)
                .ValueGeneratedNever()
                .HasColumnName("fournisseurid");
            entity.Property(e => e.Adresse)
                .HasMaxLength(256)
                .HasColumnName("adresse");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
            entity.Property(e => e.Tel)
                .HasMaxLength(256)
                .HasColumnName("tel");
        });

        modelBuilder.Entity<Loboratoire>(entity =>
        {
            entity.HasKey(e => e.Idlaboratoire).HasName("loboratoire_pk");

            entity.ToTable("loboratoire");

            entity.HasIndex(e => e.Nom, "loboratoire_uk_nom").IsUnique();

            entity.Property(e => e.Idlaboratoire).HasColumnName("idlaboratoire");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Materiel>(entity =>
        {
            entity.HasKey(e => e.Materielid).HasName("materiel_pk");

            entity.ToTable("materiel");

            entity.HasIndex(e => e.Nom, "materiel_unique").IsUnique();

            entity.HasIndex(e => e.Codebarre, "materiel_unique_1").IsUnique();

            entity.Property(e => e.Materielid)
                .ValueGeneratedNever()
                .HasColumnName("materielid");
            entity.Property(e => e.Categoriematerielid).HasColumnName("categoriematerielid");
            entity.Property(e => e.Codebarre)
                .HasMaxLength(256)
                .HasColumnName("codebarre");
            entity.Property(e => e.Conditionstockage).HasColumnName("conditionstockage");
            entity.Property(e => e.Dateperemption)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateperemption");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Idlaboratoire)
                .ValueGeneratedOnAdd()
                .HasColumnName("idlaboratoire");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Prix)
                .HasPrecision(20, 2)
                .HasColumnName("prix");
            entity.Property(e => e.Quantitemin)
                .HasPrecision(20, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("quantitemin");
            entity.Property(e => e.Zonestockage).HasColumnName("zonestockage");

            entity.HasOne(d => d.Categoriemateriel).WithMany(p => p.Materiels)
                .HasForeignKey(d => d.Categoriematerielid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("materiel_categoriemateriel_fk");

            entity.HasOne(d => d.IdlaboratoireNavigation).WithMany(p => p.Materiels)
                .HasForeignKey(d => d.Idlaboratoire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("materiel_loboratoire_fk");
        });

        modelBuilder.Entity<Methode>(entity =>
        {
            entity.HasKey(e => e.Idmethode).HasName("methode_pk");

            entity.ToTable("methode");

            entity.HasIndex(e => e.Nom, "methodeanalyse_uk_nom").IsUnique();

            entity.Property(e => e.Idmethode).HasColumnName("idmethode");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Methodeanalyse>(entity =>
        {
            entity.HasKey(e => e.Methodeanalyseid).HasName("methodeanalyse_pk");

            entity.ToTable("methodeanalyse");

            entity.HasIndex(e => new { e.Idanalyse, e.Idmethode }, "methodeanalyse_uk_methode_analyse").IsUnique();

            entity.Property(e => e.Methodeanalyseid)
                .ValueGeneratedNever()
                .HasColumnName("methodeanalyseid");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Idmethode)
                .ValueGeneratedOnAdd()
                .HasColumnName("idmethode");
            entity.Property(e => e.Isdefaultmethode)
                .HasDefaultValue(false)
                .HasColumnName("isdefaultmethode");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Methodeanalyses)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("methodeanalyse_analyse_fk");

            entity.HasOne(d => d.IdmethodeNavigation).WithMany(p => p.Methodeanalyses)
                .HasForeignKey(d => d.Idmethode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("methodeanalyse_methode_fk");
        });

        modelBuilder.Entity<Natureechantillon>(entity =>
        {
            entity.HasKey(e => e.Idnatureechantillon).HasName("natureechantillon_pk");

            entity.ToTable("natureechantillon");

            entity.HasIndex(e => e.Nom, "natureechantillon_uk_nom").IsUnique();

            entity.Property(e => e.Idnatureechantillon).HasColumnName("idnatureechantillon");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Parametre>(entity =>
        {
            entity.HasKey(e => e.Parametreid).HasName("parametre_pk");

            entity.ToTable("parametre");

            entity.Property(e => e.Parametreid)
                .ValueGeneratedNever()
                .HasColumnName("parametreid");
            entity.Property(e => e.Code)
                .HasMaxLength(32)
                .HasColumnName("code");
            entity.Property(e => e.Codeunite)
                .HasMaxLength(8)
                .HasColumnName("codeunite");
            entity.Property(e => e.Codeunitesi)
                .HasMaxLength(8)
                .HasColumnName("codeunitesi");
            entity.Property(e => e.Decimalresultatsi).HasColumnName("decimalresultatsi");
            entity.Property(e => e.Decimalresultatstandard).HasColumnName("decimalresultatstandard");
            entity.Property(e => e.Facteurconversionsi)
                .HasPrecision(18, 2)
                .HasColumnName("facteurconversionsi");
            entity.Property(e => e.Formuleautomate).HasColumnName("formuleautomate");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Masquerdansrapport)
                .HasDefaultValue(false)
                .HasColumnName("masquerdansrapport");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Ordreaffichage)
                .HasDefaultValue(0)
                .HasColumnName("ordreaffichage");
            entity.Property(e => e.Valuebuilder).HasColumnName("valuebuilder");

            entity.HasOne(d => d.CodeuniteNavigation).WithMany(p => p.ParametreCodeuniteNavigations)
                .HasForeignKey(d => d.Codeunite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parametre_unite_fk");

            entity.HasOne(d => d.CodeunitesiNavigation).WithMany(p => p.ParametreCodeunitesiNavigations)
                .HasForeignKey(d => d.Codeunitesi)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parametre_unite_fk_1");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Parametres)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parametre_analyse_fk");
        });

        modelBuilder.Entity<Partenaire>(entity =>
        {
            entity.HasKey(e => e.Partenaireid).HasName("partenaire_pk");

            entity.ToTable("partenaire");

            entity.HasIndex(e => e.Nom, "partenaire_unique").IsUnique();

            entity.Property(e => e.Partenaireid)
                .ValueGeneratedNever()
                .HasColumnName("partenaireid");
            entity.Property(e => e.Adresse).HasColumnName("adresse");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Tel)
                .HasMaxLength(128)
                .HasColumnName("tel");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Patientid).HasName("patient_pk");

            entity.ToTable("patient");

            entity.HasIndex(e => e.Code, "patient_unique").IsUnique();

            entity.Property(e => e.Patientid)
                .ValueGeneratedNever()
                .HasColumnName("patientid");
            entity.Property(e => e.Adresse)
                .HasMaxLength(256)
                .HasColumnName("adresse");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.Code)
                .HasMaxLength(128)
                .HasColumnName("code");
            entity.Property(e => e.Codesexe)
                .HasMaxLength(8)
                .HasColumnName("codesexe");
            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Codetypedocumentidentite)
                .HasMaxLength(8)
                .HasColumnName("codetypedocumentidentite");
            entity.Property(e => e.Codetypepeau)
                .HasMaxLength(16)
                .HasColumnName("codetypepeau");
            entity.Property(e => e.Createdby)
                .HasMaxLength(64)
                .HasColumnName("createdby");
            entity.Property(e => e.Creationdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("creationdate");
            entity.Property(e => e.Datenaissance)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datenaissance");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Lieunaissance)
                .HasMaxLength(256)
                .HasColumnName("lieunaissance");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
            entity.Property(e => e.Nomusage)
                .HasMaxLength(256)
                .HasColumnName("nomusage");
            entity.Property(e => e.Numerodocumentidentite)
                .HasMaxLength(64)
                .HasColumnName("numerodocumentidentite");
            entity.Property(e => e.Prenom)
                .HasMaxLength(256)
                .HasColumnName("prenom");
            entity.Property(e => e.Quartier)
                .HasMaxLength(128)
                .HasColumnName("quartier");
            entity.Property(e => e.Renseignementclinique).HasColumnName("renseignementclinique");
            entity.Property(e => e.Tel)
                .HasMaxLength(32)
                .HasColumnName("tel");
            entity.Property(e => e.Updateby)
                .HasMaxLength(64)
                .HasColumnName("updateby");
            entity.Property(e => e.Updatedate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updatedate");
            entity.Property(e => e.Ville)
                .HasMaxLength(128)
                .HasColumnName("ville");

            entity.HasOne(d => d.CodesexeNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.Codesexe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_sexe_fk");

            entity.HasOne(d => d.CodesiteNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.Codesite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_site_fk");

            entity.HasOne(d => d.CodetypedocumentidentiteNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.Codetypedocumentidentite)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("patient_typedocumentidentite_fk");

            entity.HasOne(d => d.CodetypepeauNavigation).WithMany(p => p.Patients)
                .HasForeignKey(d => d.Codetypepeau)
                .HasConstraintName("patient_typepeau_fk");
        });

        modelBuilder.Entity<Photopatient>(entity =>
        {
            entity.HasKey(e => e.Photopatientid).HasName("photopatient_pk");

            entity.ToTable("photopatient");

            entity.Property(e => e.Photopatientid)
                .ValueGeneratedNever()
                .HasColumnName("photopatientid");
            entity.Property(e => e.Extension)
                .HasMaxLength(8)
                .HasColumnName("extension");
            entity.Property(e => e.Patientid).HasColumnName("patientid");
            entity.Property(e => e.Photo).HasColumnName("photo");

            entity.HasOne(d => d.Patient).WithMany(p => p.Photopatients)
                .HasForeignKey(d => d.Patientid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("photopatient_patient_fk");
        });

        modelBuilder.Entity<Policeassurance>(entity =>
        {
            entity.HasKey(e => e.Policeassuranceid).HasName("policeassurance_pk");

            entity.ToTable("policeassurance");

            entity.Property(e => e.Policeassuranceid)
                .ValueGeneratedNever()
                .HasColumnName("policeassuranceid");
            entity.Property(e => e.Codeassurance)
                .HasMaxLength(16)
                .HasColumnName("codeassurance");
            entity.Property(e => e.Libelle)
                .HasMaxLength(128)
                .HasColumnName("libelle");
            entity.Property(e => e.Taux)
                .HasPrecision(20, 2)
                .HasColumnName("taux");

            entity.HasOne(d => d.CodeassuranceNavigation).WithMany(p => p.Policeassurances)
                .HasForeignKey(d => d.Codeassurance)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("policeassurance_assurance_fk");
        });

        modelBuilder.Entity<Prelevement>(entity =>
        {
            entity.HasKey(e => e.Prelevementid).HasName("prelevement_pk");

            entity.ToTable("prelevement");

            entity.Property(e => e.Prelevementid)
                .ValueGeneratedNever()
                .HasColumnName("prelevementid");
            entity.Property(e => e.Dateprelevement)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateprelevement");
            entity.Property(e => e.Datereception)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datereception");
            entity.Property(e => e.Detaildemandeid).HasColumnName("detaildemandeid");
            entity.Property(e => e.Idnatureechantillon)
                .ValueGeneratedOnAdd()
                .HasColumnName("idnatureechantillon");
            entity.Property(e => e.Lieuprelevement)
                .HasMaxLength(128)
                .HasColumnName("lieuprelevement");
            entity.Property(e => e.Lieureception)
                .HasMaxLength(128)
                .HasColumnName("lieureception");
            entity.Property(e => e.Preleveurid).HasColumnName("preleveurid");
            entity.Property(e => e.Renseignementclinique).HasColumnName("renseignementclinique");
            entity.Property(e => e.Siteanatomique)
                .HasMaxLength(128)
                .HasColumnName("siteanatomique");
            entity.Property(e => e.Statut)
                .HasMaxLength(32)
                .HasColumnName("statut");

            entity.HasOne(d => d.Detaildemande).WithMany(p => p.Prelevements)
                .HasForeignKey(d => d.Detaildemandeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prelevement_detaildemande_fk");

            entity.HasOne(d => d.IdnatureechantillonNavigation).WithMany(p => p.Prelevements)
                .HasForeignKey(d => d.Idnatureechantillon)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prelevement_natureechantillon_fk");

            entity.HasOne(d => d.Preleveur).WithMany(p => p.Prelevements)
                .HasForeignKey(d => d.Preleveurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("prelevement_preleveur_fk");
        });

        modelBuilder.Entity<Preleveur>(entity =>
        {
            entity.HasKey(e => e.Preleveurid).HasName("preleveur_pk");

            entity.ToTable("preleveur");

            entity.Property(e => e.Preleveurid)
                .ValueGeneratedNever()
                .HasColumnName("preleveurid");
            entity.Property(e => e.Codesexe)
                .HasMaxLength(10)
                .HasColumnName("codesexe");
            entity.Property(e => e.Datenaissance).HasColumnName("datenaissance");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasColumnName("email");
            entity.Property(e => e.Fonction)
                .HasMaxLength(32)
                .HasColumnName("fonction");
            entity.Property(e => e.Mob1)
                .HasMaxLength(32)
                .HasColumnName("mob1");
            entity.Property(e => e.Mob2)
                .HasMaxLength(32)
                .HasColumnName("mob2");
            entity.Property(e => e.Nom)
                .HasMaxLength(64)
                .HasColumnName("nom");
            entity.Property(e => e.Prenom)
                .HasMaxLength(64)
                .HasColumnName("prenom");
            entity.Property(e => e.Tel1)
                .HasMaxLength(32)
                .HasColumnName("tel1");
            entity.Property(e => e.Tel2)
                .HasMaxLength(32)
                .HasColumnName("tel2");

            entity.HasOne(d => d.CodesexeNavigation).WithMany(p => p.Preleveurs)
                .HasForeignKey(d => d.Codesexe)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("preleveur_sexe_fk");
        });

        modelBuilder.Entity<Prescripteur>(entity =>
        {
            entity.HasKey(e => e.Prescripteurid).HasName("prescripteur_pk");

            entity.ToTable("prescripteur");

            entity.Property(e => e.Prescripteurid)
                .ValueGeneratedNever()
                .HasColumnName("prescripteurid");
            entity.Property(e => e.Adresse).HasColumnName("adresse");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
            entity.Property(e => e.Tel)
                .HasMaxLength(128)
                .HasColumnName("tel");
        });

        modelBuilder.Entity<Profil>(entity =>
        {
            entity.HasKey(e => e.Profilid).HasName("profil_pk");

            entity.ToTable("profil");

            entity.HasIndex(e => e.Nom, "profil_unique").IsUnique();

            entity.Property(e => e.Profilid)
                .HasMaxLength(8)
                .HasColumnName("profilid");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(256)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Sexe>(entity =>
        {
            entity.HasKey(e => e.Codesexe).HasName("gender_pk");

            entity.ToTable("sexe");

            entity.Property(e => e.Codesexe)
                .HasMaxLength(8)
                .HasColumnName("codesexe");
            entity.Property(e => e.Value)
                .HasColumnType("character varying")
                .HasColumnName("value");
        });

        modelBuilder.Entity<Sexeautorise>(entity =>
        {
            entity.HasKey(e => e.Sexeautorisecode).HasName("sexeautorise_pk");

            entity.ToTable("sexeautorise");

            entity.HasIndex(e => e.Valeur, "sexeautorise_uk_valeur").IsUnique();

            entity.Property(e => e.Sexeautorisecode)
                .HasMaxLength(8)
                .HasColumnName("sexeautorisecode");
            entity.Property(e => e.Valeur)
                .HasMaxLength(128)
                .HasColumnName("valeur");
        });

        modelBuilder.Entity<Signatureutilisateur>(entity =>
        {
            entity.HasKey(e => e.Signatureutilisateurid).HasName("signatureutilisateur_pk");

            entity.ToTable("signatureutilisateur");

            entity.Property(e => e.Signatureutilisateurid)
                .ValueGeneratedNever()
                .HasColumnName("signatureutilisateurid");
            entity.Property(e => e.Extension)
                .HasMaxLength(8)
                .HasColumnName("extension");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Signatureutilisateurs)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("signatureutilisateur_utilisateur_fk");
        });

        modelBuilder.Entity<Site>(entity =>
        {
            entity.HasKey(e => e.Codesite).HasName("site_pk");

            entity.ToTable("site");

            entity.HasIndex(e => e.Codesite, "location_code").IsUnique();

            entity.HasIndex(e => e.Name, "location_name").IsUnique();

            entity.HasIndex(e => e.Name, "site_unique").IsUnique();

            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Name)
                .HasColumnType("character varying")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Tarifanalyseassurance>(entity =>
        {
            entity.HasKey(e => e.Tarifanalyseassuranceid).HasName("tarifanalyseassurance_pk");

            entity.ToTable("tarifanalyseassurance");

            entity.HasIndex(e => new { e.Idanalyse, e.Codeassurance }, "tarifanalyseassurance_unique").IsUnique();

            entity.Property(e => e.Tarifanalyseassuranceid)
                .ValueGeneratedNever()
                .HasColumnName("tarifanalyseassuranceid");
            entity.Property(e => e.Codeassurance)
                .HasMaxLength(16)
                .HasColumnName("codeassurance");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Prix)
                .HasPrecision(18, 2)
                .HasColumnName("prix");

            entity.HasOne(d => d.CodeassuranceNavigation).WithMany(p => p.Tarifanalyseassurances)
                .HasForeignKey(d => d.Codeassurance)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tarifanalyseassurance_assurance_fk");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Tarifanalyseassurances)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tarifanalyseassurance_analyse_fk");
        });

        modelBuilder.Entity<Tarifcategorieassurance>(entity =>
        {
            entity.HasKey(e => e.Tarifcategorieassuranceid).HasName("tarifcategorieassurance_pk");

            entity.ToTable("tarifcategorieassurance");

            entity.HasIndex(e => new { e.Categorieid, e.Codeassurance }, "tarifcategorieassurance_unique").IsUnique();

            entity.Property(e => e.Tarifcategorieassuranceid)
                .ValueGeneratedNever()
                .HasColumnName("tarifcategorieassuranceid");
            entity.Property(e => e.Categorieid).HasColumnName("categorieid");
            entity.Property(e => e.Codeassurance)
                .HasMaxLength(16)
                .HasColumnName("codeassurance");
            entity.Property(e => e.Prix)
                .HasPrecision(18, 2)
                .HasColumnName("prix");

            entity.HasOne(d => d.Categorie).WithMany(p => p.Tarifcategorieassurances)
                .HasForeignKey(d => d.Categorieid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tarifcategorieassurance_categorie_fk");

            entity.HasOne(d => d.CodeassuranceNavigation).WithMany(p => p.Tarifcategorieassurances)
                .HasForeignKey(d => d.Codeassurance)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("tarifcategorieassurance_assurance_fk");
        });

        modelBuilder.Entity<Typeassurance>(entity =>
        {
            entity.HasKey(e => e.Codetypeassurance).HasName("typeassurance_pk");

            entity.ToTable("typeassurance");

            entity.HasIndex(e => e.Nom, "typeassurance_unique").IsUnique();

            entity.Property(e => e.Codetypeassurance)
                .HasMaxLength(8)
                .HasColumnName("codetypeassurance");
            entity.Property(e => e.Nom)
                .HasMaxLength(28)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Typedocumentidentite>(entity =>
        {
            entity.HasKey(e => e.Codetypedocumentidentite).HasName("typedocumentidentite_pk");

            entity.ToTable("typedocumentidentite");

            entity.Property(e => e.Codetypedocumentidentite)
                .HasMaxLength(8)
                .HasColumnName("codetypedocumentidentite");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Typepeau>(entity =>
        {
            entity.HasKey(e => e.Codetypepeau).HasName("typepeau_pk");

            entity.ToTable("typepeau");

            entity.Property(e => e.Codetypepeau)
                .HasMaxLength(16)
                .HasColumnName("codetypepeau");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Nom)
                .HasMaxLength(128)
                .HasColumnName("nom");
        });

        modelBuilder.Entity<Unite>(entity =>
        {
            entity.HasKey(e => e.Code).HasName("unite_pk");

            entity.ToTable("unite");

            entity.HasIndex(e => e.Name, "unite_uk_name").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(8)
                .HasColumnName("code");
            entity.Property(e => e.Isage).HasColumnName("isage");
            entity.Property(e => e.Isageday).HasColumnName("isageday");
            entity.Property(e => e.Name)
                .HasMaxLength(32)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.Utilisateurid).HasName("utilisateur_pk");

            entity.ToTable("utilisateur");

            entity.Property(e => e.Utilisateurid)
                .ValueGeneratedNever()
                .HasColumnName("utilisateurid");
            entity.Property(e => e.Codesexe)
                .HasMaxLength(10)
                .HasColumnName("codesexe");
            entity.Property(e => e.Codesite)
                .HasColumnType("character varying")
                .HasColumnName("codesite");
            entity.Property(e => e.Createdby)
                .HasMaxLength(64)
                .HasColumnName("createdby");
            entity.Property(e => e.Creationdate).HasColumnName("creationdate");
            entity.Property(e => e.Datenaissance).HasColumnName("datenaissance");
            entity.Property(e => e.Email)
                .HasMaxLength(64)
                .HasColumnName("email");
            entity.Property(e => e.Idinh)
                .HasMaxLength(32)
                .HasColumnName("idinh");
            entity.Property(e => e.Isactive).HasColumnName("isactive");
            entity.Property(e => e.Login)
                .HasMaxLength(64)
                .HasColumnName("login");
            entity.Property(e => e.Matricule)
                .HasMaxLength(64)
                .HasColumnName("matricule");
            entity.Property(e => e.Mob1)
                .HasMaxLength(32)
                .HasColumnName("mob1");
            entity.Property(e => e.Mob2)
                .HasMaxLength(32)
                .HasColumnName("mob2");
            entity.Property(e => e.Mustchangepass).HasColumnName("mustchangepass");
            entity.Property(e => e.Nationnalite)
                .HasMaxLength(2)
                .HasColumnName("nationnalite");
            entity.Property(e => e.Nom)
                .HasMaxLength(64)
                .HasColumnName("nom");
            entity.Property(e => e.Password)
                .HasMaxLength(256)
                .HasColumnName("password");
            entity.Property(e => e.Prenom)
                .HasMaxLength(64)
                .HasColumnName("prenom");
            entity.Property(e => e.Tel1)
                .HasMaxLength(32)
                .HasColumnName("tel1");
            entity.Property(e => e.Tel2)
                .HasMaxLength(32)
                .HasColumnName("tel2");
            entity.Property(e => e.Updateby)
                .HasMaxLength(64)
                .HasColumnName("updateby");
            entity.Property(e => e.Updatedate).HasColumnName("updatedate");
            entity.Property(e => e.Userid)
                .HasMaxLength(450)
                .HasColumnName("userid");

            entity.HasOne(d => d.CodesiteNavigation).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.Codesite)
                .HasConstraintName("utilisateur_site_fk");

            entity.HasOne(d => d.NationnaliteNavigation).WithMany(p => p.Utilisateurs)
                .HasForeignKey(d => d.Nationnalite)
                .HasConstraintName("utilisateur_country_fk");
        });

        modelBuilder.Entity<Utilisateurlaboratoire>(entity =>
        {
            entity.HasKey(e => e.Utilisateurlaboratoireid).HasName("utilisateurlaboratoire_pk");

            entity.ToTable("utilisateurlaboratoire");

            entity.Property(e => e.Utilisateurlaboratoireid)
                .ValueGeneratedNever()
                .HasColumnName("utilisateurlaboratoireid");
            entity.Property(e => e.Idlaboratoire)
                .ValueGeneratedOnAdd()
                .HasColumnName("idlaboratoire");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.IdlaboratoireNavigation).WithMany(p => p.Utilisateurlaboratoires)
                .HasForeignKey(d => d.Idlaboratoire)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("utilisateurlaboratoire_loboratoire_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Utilisateurlaboratoires)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("utilisateurlaboratoire_utilisateur_fk");
        });

        modelBuilder.Entity<Utilisateurprofil>(entity =>
        {
            entity.HasKey(e => e.Utilisateurprofilid).HasName("utilisateurprofil_pk");

            entity.ToTable("utilisateurprofil");

            entity.Property(e => e.Utilisateurprofilid)
                .ValueGeneratedNever()
                .HasColumnName("utilisateurprofilid");
            entity.Property(e => e.Profilid)
                .HasMaxLength(8)
                .HasColumnName("profilid");
            entity.Property(e => e.Utilisateurid).HasColumnName("utilisateurid");

            entity.HasOne(d => d.Profil).WithMany(p => p.Utilisateurprofils)
                .HasForeignKey(d => d.Profilid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("utilisateurprofil_profil_fk");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Utilisateurprofils)
                .HasForeignKey(d => d.Utilisateurid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("utilisateurprofil_utilisateur_fk");
        });

        modelBuilder.Entity<Valeurreference>(entity =>
        {
            entity.HasKey(e => e.Valeurreferenceid).HasName("valeurreference_pk");

            entity.ToTable("valeurreference");

            entity.Property(e => e.Valeurreferenceid)
                .ValueGeneratedNever()
                .HasColumnName("valeurreferenceid");
            entity.Property(e => e.Agedebut).HasColumnName("agedebut");
            entity.Property(e => e.Agefin).HasColumnName("agefin");
            entity.Property(e => e.Codeunitagedebut)
                .HasMaxLength(8)
                .HasColumnName("codeunitagedebut");
            entity.Property(e => e.Codeunitagefin)
                .HasMaxLength(8)
                .HasColumnName("codeunitagefin");
            entity.Property(e => e.Codeunitreference)
                .HasMaxLength(8)
                .HasColumnName("codeunitreference");
            entity.Property(e => e.Codeunitreferencesi)
                .HasMaxLength(8)
                .HasColumnName("codeunitreferencesi");
            entity.Property(e => e.Idanalyse).HasColumnName("idanalyse");
            entity.Property(e => e.Referencefromvalue)
                .HasPrecision(18, 2)
                .HasColumnName("referencefromvalue");
            entity.Property(e => e.Referencefromvaluesi)
                .HasPrecision(18, 2)
                .HasColumnName("referencefromvaluesi");
            entity.Property(e => e.Referencetovalue)
                .HasPrecision(18, 2)
                .HasColumnName("referencetovalue");
            entity.Property(e => e.Referencetovaluesi)
                .HasPrecision(18, 2)
                .HasColumnName("referencetovaluesi");
            entity.Property(e => e.Sexeautorisecode)
                .HasMaxLength(8)
                .HasColumnName("sexeautorisecode");
            entity.Property(e => e.Titre)
                .HasMaxLength(256)
                .HasColumnName("titre");

            entity.HasOne(d => d.CodeunitagedebutNavigation).WithMany(p => p.ValeurreferenceCodeunitagedebutNavigations)
                .HasForeignKey(d => d.Codeunitagedebut)
                .HasConstraintName("valeurreference_unite_fk_agedebut");

            entity.HasOne(d => d.CodeunitagefinNavigation).WithMany(p => p.ValeurreferenceCodeunitagefinNavigations)
                .HasForeignKey(d => d.Codeunitagefin)
                .HasConstraintName("valeurreference_unite_fk_age_fin");

            entity.HasOne(d => d.CodeunitreferenceNavigation).WithMany(p => p.ValeurreferenceCodeunitreferenceNavigations)
                .HasForeignKey(d => d.Codeunitreference)
                .HasConstraintName("valeurreference_unite_fk_reference");

            entity.HasOne(d => d.CodeunitreferencesiNavigation).WithMany(p => p.ValeurreferenceCodeunitreferencesiNavigations)
                .HasForeignKey(d => d.Codeunitreferencesi)
                .HasConstraintName("valeurreference_unite_fk_referencesi");

            entity.HasOne(d => d.IdanalyseNavigation).WithMany(p => p.Valeurreferences)
                .HasForeignKey(d => d.Idanalyse)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("valeurreference_analyse_fk");

            entity.HasOne(d => d.SexeautorisecodeNavigation).WithMany(p => p.Valeurreferences)
                .HasForeignKey(d => d.Sexeautorisecode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("valeurreference_sexeautorise_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
