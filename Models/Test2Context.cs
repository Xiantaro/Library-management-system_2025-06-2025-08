using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using test2.Models.ManagementModels.Services;
using test2.Models.ManagementModels.ZhongXian.Appoimtment;
using test2.Models.ManagementModels.ZhongXian.BookQuery;
using test2.Models.ManagementModels.ZhongXian.Borrow;
using test2.Models.ManagementModels.ZhongXian.Normal;
using test2.Models.ManagementModels.ZhongXian.ReturnBook;

namespace test2.Models;

public partial class Test2Context : DbContext
{
    public Test2Context()
    {
    }

    public Test2Context(DbContextOptions<Test2Context> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityType> ActivityTypes { get; set; }

    public virtual DbSet<Announcement> Announcements { get; set; }

    public virtual DbSet<AnnouncementType> AnnouncementTypes { get; set; }

    public virtual DbSet<Audience> Audiences { get; set; }

    public virtual DbSet<Author> Authors { get; set; }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<BookStatus> BookStatuses { get; set; }

    public virtual DbSet<Borrow> Borrows { get; set; }

    public virtual DbSet<BorrowStatus> BorrowStatuses { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Collection> Collections { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<History> Histories { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Participation> Participations { get; set; }

    public virtual DbSet<ParticipationStatus> ParticipationStatuses { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<ReservationStatus> ReservationStatuses { get; set; }

    public virtual DbSet<Type> Types { get; set; }

    // custom
    public virtual DbSet<Type> cId { get; set; }
    public virtual DbSet<Type> collectionId { get; set; }
    public virtual DbSet<Type> reservationDate { get; set; }
    public virtual DbSet<Type> reservationStatusId { get; set; }
    public virtual DbSet<Type> score { get; set; }
    public virtual DbSet<Type> feedback { get; set; }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder.UseSqlServer("Server=DESKTOP-I57EDSL\\SQLEXPRESS;Database=test2;Integrated Security=True;Trusted_Connection=True;TrustServerCertificate=True;User ID=sa;Password=1234;");

    // 憲custom
    public virtual DbSet<AppoimtmentKeywordDTO> AppoimtmentKeywordShows { get; set; }
    public virtual DbSet<BorrowBookInfomationDTO> BorrowBookInfomationDTOs { get; set; }
    public virtual DbSet<MessageDTO> BorrwoMessageDTOs { get; set; }
    public virtual DbSet<MessageDTO2> MessageDTO2 { get; set; }
    public virtual DbSet<LanguageAndTypeViewModel> LanguageAndTypes { get; set; }
    public virtual DbSet<BookQueryDTO> BookQueryDTOs { get; set; }
    public virtual DbSet<NotificationUserDTO> NotificationUserDTOs { get; set; }
    public virtual DbSet<ReturnDTO> LateReturnDTOs { get; set; }
    public virtual DbSet<OverDueDTO> OverDueDTOs { get; set; }
    public virtual DbSet<ReturnBookDTO> ReturnBookDTOs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.ToTable("Activity");

            entity.HasIndex(e => e.ActivityTitle, "UQ__Activity__5F9FE5733FADB099").IsUnique();

            entity.Property(e => e.ActivityId).HasColumnName("activityId");
            entity.Property(e => e.ActivityDesc).HasColumnName("activityDesc");
            entity.Property(e => e.ActivityImg).HasColumnName("activityImg");
            entity.Property(e => e.ActivityTitle)
                .HasMaxLength(100)
                .HasColumnName("activityTitle");
            entity.Property(e => e.ActivityTypeId).HasColumnName("activityTypeId");
            entity.Property(e => e.AudienceId).HasColumnName("audienceId");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.EndDate).HasColumnName("endDate");
            entity.Property(e => e.StartDate).HasColumnName("startDate");

            entity.HasOne(d => d.ActivityType).WithMany(p => p.Activities)
                .HasForeignKey(d => d.ActivityTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ActivityType");

            entity.HasOne(d => d.Audience).WithMany(p => p.Activities)
                .HasForeignKey(d => d.AudienceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Audience");
        });

        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.ToTable("ActivityType");

            entity.HasIndex(e => e.ActivityType1, "UQ__Activity__1F1EE4DDCFFFAE6E").IsUnique();

            entity.Property(e => e.ActivityTypeId).HasColumnName("activityTypeId");
            entity.Property(e => e.ActivityType1)
                .HasMaxLength(50)
                .HasColumnName("activityType");
        });

        modelBuilder.Entity<Announcement>(entity =>
        {
            entity.ToTable("Announcement");

            entity.HasIndex(e => e.AnnouncementTitle, "UQ__Announce__EBE0015F579A2769").IsUnique();

            entity.Property(e => e.AnnouncementId).HasColumnName("announcementId");
            entity.Property(e => e.AnnouncementDesc).HasColumnName("announcementDesc");
            entity.Property(e => e.AnnouncementTitle)
                .HasMaxLength(100)
                .HasColumnName("announcementTitle");
            entity.Property(e => e.AnnouncementTypeId).HasColumnName("announcementTypeId");

            entity.HasOne(d => d.AnnouncementType).WithMany(p => p.Announcements)
                .HasForeignKey(d => d.AnnouncementTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AnnouncementType");
        });

        modelBuilder.Entity<AnnouncementType>(entity =>
        {
            entity.ToTable("AnnouncementType");

            entity.HasIndex(e => e.AnnouncementType1, "UQ__Announce__D53C87B22984DEC9").IsUnique();

            entity.Property(e => e.AnnouncementTypeId).HasColumnName("announcementTypeId");
            entity.Property(e => e.AnnouncementType1)
                .HasMaxLength(50)
                .HasColumnName("announcementType");
        });

        modelBuilder.Entity<Audience>(entity =>
        {
            entity.ToTable("Audience");

            entity.HasIndex(e => e.Audience1, "UQ__Audience__2C1B51FCF15D941B").IsUnique();

            entity.Property(e => e.AudienceId).HasColumnName("audienceId");
            entity.Property(e => e.Audience1)
                .HasMaxLength(50)
                .HasColumnName("audience");
        });

        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("Author");

            entity.HasIndex(e => e.Author1, "UQ_Author_Name").IsUnique();

            entity.Property(e => e.AuthorId).HasColumnName("authorId");
            entity.Property(e => e.Author1)
                .HasMaxLength(50)
                .HasColumnName("author");
            entity.Property(e => e.AuthorDesc).HasColumnName("authorDesc");
        });

        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("Book");

            entity.HasIndex(e => e.BookCode, "UQ__Book__3BB8DAE6F11F54D2").IsUnique();

            entity.Property(e => e.BookId).HasColumnName("bookId");
            entity.Property(e => e.AccessionDate).HasColumnName("accessionDate");
            entity.Property(e => e.BookCode)
                .HasMaxLength(23)
                .IsUnicode(false)
                .HasColumnName("bookCode");
            entity.Property(e => e.BookStatusId).HasColumnName("bookStatusId");
            entity.Property(e => e.CollectionId).HasColumnName("collectionId");

            entity.HasOne(d => d.BookStatus).WithMany(p => p.Books)
                .HasForeignKey(d => d.BookStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookStatus");

            entity.HasOne(d => d.Collection).WithMany(p => p.Books)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionB");
        });

        modelBuilder.Entity<BookStatus>(entity =>
        {
            entity.ToTable("BookStatus");

            entity.HasIndex(e => e.BookStatus1, "UQ__BookStat__6890475C2DC102B3").IsUnique();

            entity.Property(e => e.BookStatusId).HasColumnName("bookStatusId");
            entity.Property(e => e.BookStatus1)
                .HasMaxLength(50)
                .HasColumnName("bookStatus");
        });

        modelBuilder.Entity<Borrow>(entity =>
        {
            entity.ToTable("Borrow");

            entity.Property(e => e.BorrowId).HasColumnName("borrowId");
            entity.Property(e => e.BookId).HasColumnName("bookId");
            entity.Property(e => e.BorrowDate).HasColumnName("borrowDate");
            entity.Property(e => e.BorrowStatusId).HasColumnName("borrowStatusId");
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.DueDateB).HasColumnName("dueDateB");
            entity.Property(e => e.ReservationId).HasColumnName("reservationId");
            entity.Property(e => e.ReturnDate).HasColumnName("returnDate");

            entity.HasOne(d => d.Book).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BookB");

            entity.HasOne(d => d.BorrowStatus).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.BorrowStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_BorrowStatus");

            entity.HasOne(d => d.CIdNavigation).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.CId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientB");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Borrows)
                .HasForeignKey(d => d.ReservationId)
                .HasConstraintName("FK_Reservation");
        });

        modelBuilder.Entity<BorrowStatus>(entity =>
        {
            entity.ToTable("BorrowStatus");

            entity.HasIndex(e => e.BorrowStatus1, "UQ__BorrowSt__EF08F8F4B409EFC7").IsUnique();

            entity.Property(e => e.BorrowStatusId).HasColumnName("borrowStatusId");
            entity.Property(e => e.BorrowStatus1)
                .HasMaxLength(50)
                .HasColumnName("borrowStatus");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.CId);

            entity.ToTable("Client");

            entity.HasIndex(e => e.CPhone, "UQ__Client__0A376ADC5C14DAE8").IsUnique();

            entity.HasIndex(e => e.CAccount, "UQ__Client__2F046D2399440AC9").IsUnique();

            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.CAccount)
                .HasMaxLength(100)
                .HasColumnName("cAccount");
            entity.Property(e => e.CName)
                .HasMaxLength(50)
                .HasColumnName("cName");
            entity.Property(e => e.CPassword)
                .HasMaxLength(255)
                .HasColumnName("cPassword");
            entity.Property(e => e.CPhone)
                .HasMaxLength(20)
                .HasColumnName("cPhone");
            entity.Property(e => e.FacebookId).HasMaxLength(255);
            entity.Property(e => e.GoogleId).HasMaxLength(255);
            entity.Property(e => e.Permission).HasColumnName("permission");
        });

        modelBuilder.Entity<Collection>(entity =>
        {
            entity.ToTable("Collection");

            entity.HasIndex(e => e.Isbn, "UQ__Collecti__99F9D0A4B36171AF").IsUnique();

            entity.Property(e => e.CollectionId).HasColumnName("collectionId");
            entity.Property(e => e.AuthorId).HasColumnName("authorId");
            entity.Property(e => e.CollectionDesc).HasColumnName("collectionDesc");
            entity.Property(e => e.CollectionImg).HasColumnName("collectionImg");
            entity.Property(e => e.Isbn)
                .HasMaxLength(17)
                .IsUnicode(false)
                .HasColumnName("isbn");
            entity.Property(e => e.LanguageId).HasColumnName("languageId");
            entity.Property(e => e.PublishDate).HasColumnName("publishDate");
            entity.Property(e => e.Publisher)
                .HasMaxLength(50)
                .HasColumnName("publisher");
            entity.Property(e => e.Title)
                .HasMaxLength(100)
                .HasColumnName("title");
            entity.Property(e => e.Translator)
                .HasMaxLength(50)
                .HasColumnName("translator");
            entity.Property(e => e.TypeId).HasColumnName("typeId");

            entity.HasOne(d => d.Author).WithMany(p => p.Collections)
                .HasForeignKey(d => d.AuthorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Author");

            entity.HasOne(d => d.Language).WithMany(p => p.Collections)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Language");

            entity.HasOne(d => d.Type).WithMany(p => p.Collections)
                .HasForeignKey(d => d.TypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Type");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoritesId);

            entity.HasIndex(e => new { e.CId, e.CollectionId }, "CUC_Favorites").IsUnique();

            entity.Property(e => e.FavoritesId).HasColumnName("favoritesId");
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.CollectionId).HasColumnName("collectionId");

            entity.HasOne(d => d.CIdNavigation).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.CId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientF");

            entity.HasOne(d => d.Collection).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionF");
        });

        modelBuilder.Entity<History>(entity =>
        {
            entity.ToTable("History");

            entity.Property(e => e.HistoryId).HasColumnName("historyId");
            entity.Property(e => e.BorrowId).HasColumnName("borrowId");
            entity.Property(e => e.Feedback).HasColumnName("feedback");
            entity.Property(e => e.Score).HasColumnName("score");

            entity.HasOne(d => d.Borrow).WithMany(p => p.Histories)
                .HasForeignKey(d => d.BorrowId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Borrow");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.ToTable("Language");

            entity.HasIndex(e => e.Language1, "UQ__Language__EFADA5D93D30595A").IsUnique();

            entity.Property(e => e.LanguageId).HasColumnName("languageId");
            entity.Property(e => e.Language1)
                .HasMaxLength(50)
                .HasColumnName("language");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");

            entity.Property(e => e.NotificationId).HasColumnName("notificationId");
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.NotificationDate).HasColumnName("notificationDate");

            entity.HasOne(d => d.CIdNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.CId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientN");
        });

        modelBuilder.Entity<Participation>(entity =>
        {
            entity.ToTable("Participation");

            entity.Property(e => e.ParticipationId).HasColumnName("participationId");
            entity.Property(e => e.ActivityId).HasColumnName("activityId");
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.ParticipationDate).HasColumnName("participationDate");
            entity.Property(e => e.ParticipationStatusId).HasColumnName("participationStatusId");

            entity.HasOne(d => d.Activity).WithMany(p => p.Participations)
                .HasForeignKey(d => d.ActivityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activity");

            entity.HasOne(d => d.CIdNavigation).WithMany(p => p.Participations)
                .HasForeignKey(d => d.CId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientP");

            entity.HasOne(d => d.ParticipationStatus).WithMany(p => p.Participations)
                .HasForeignKey(d => d.ParticipationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ParticipationStatus");
        });

        modelBuilder.Entity<ParticipationStatus>(entity =>
        {
            entity.ToTable("ParticipationStatus");

            entity.Property(e => e.ParticipationStatusId).HasColumnName("participationStatusId");
            entity.Property(e => e.ParticipationStatus1)
                .HasMaxLength(50)
                .HasColumnName("participationStatus");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservation");

            entity.Property(e => e.ReservationId).HasColumnName("reservationId");
            entity.Property(e => e.BookId).HasColumnName("bookId");
            entity.Property(e => e.CId).HasColumnName("cId");
            entity.Property(e => e.CollectionId).HasColumnName("collectionId");
            entity.Property(e => e.DueDateR).HasColumnName("dueDateR");
            entity.Property(e => e.ReservationDate).HasColumnName("reservationDate");
            entity.Property(e => e.ReservationStatusId).HasColumnName("reservationStatusId");

            entity.HasOne(d => d.Book).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK_BookR");

            entity.HasOne(d => d.CIdNavigation).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ClientR");

            entity.HasOne(d => d.Collection).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.CollectionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CollectionR");

            entity.HasOne(d => d.ReservationStatus).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.ReservationStatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ReservationStatus");
        });

        modelBuilder.Entity<ReservationStatus>(entity =>
        {
            entity.ToTable("ReservationStatus");

            entity.HasIndex(e => e.ReservationStatus1, "UQ__Reservat__C351879309FCEE92").IsUnique();

            entity.Property(e => e.ReservationStatusId).HasColumnName("reservationStatusId");
            entity.Property(e => e.ReservationStatus1)
                .HasMaxLength(50)
                .HasColumnName("reservationStatus");
        });

        modelBuilder.Entity<Type>(entity =>
        {
            entity.ToTable("Type");

            entity.HasIndex(e => e.Type1, "UQ__Type__E3F8524832E04BDF").IsUnique();

            entity.Property(e => e.TypeId).HasColumnName("typeId");
            entity.Property(e => e.Type1)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        // 憲custom
        modelBuilder.Entity<BorrowBookInfomationDTO>().HasNoKey();
        modelBuilder.Entity<MessageDTO>().HasNoKey();
        modelBuilder.Entity<MessageDTO2>().HasNoKey();
        modelBuilder.Entity<AppoimtmentKeywordDTO>().HasNoKey();
        modelBuilder.Entity<LanguageAndTypeViewModel>().HasNoKey();
        modelBuilder.Entity<BookQueryDTO>().HasNoKey();
        modelBuilder.Entity<NotificationUserDTO>().HasNoKey();
        modelBuilder.Entity<ReturnDTO>().HasNoKey();
        modelBuilder.Entity<OverDueDTO>().HasNoKey();
        modelBuilder.Entity<ReturnBookDTO>().HasNoKey();

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
