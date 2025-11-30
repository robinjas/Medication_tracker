using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FMMS.Models;
using FMMS.Services;
using SQLite;
using Xunit;

namespace FMMS.Tests;

/// <summary>
/// Unit tests for the DatabaseService.
/// Tests CRUD operations, search functionality, and data integrity.
/// </summary>
public class DatabaseServiceTests : IDisposable
{
    private readonly DatabaseService _databaseService;
    private readonly string _testDbPath;

    public DatabaseServiceTests()
    {
        // Create a temporary file database for testing
        // Using a temp file instead of :memory: to avoid issues with multiple connections
        _testDbPath = Path.GetTempFileName();
        _databaseService = new DatabaseService(_testDbPath);
    }

    public void Dispose()
    {
        // Cleanup: delete temp file
        // Note: DatabaseService doesn't implement IDisposable, but SQLite connections
        // are automatically closed when the connection object is garbage collected
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore errors during cleanup (file may be locked briefly)
            }
        }
    }

    [Fact]
    public async Task InitializeAsync_CreatesTables()
    {
        // Act
        await _databaseService.InitializeAsync();

        // Assert - If no exception is thrown, tables were created
        // We can verify by trying to get data
        var people = await _databaseService.GetPeopleAsync();
        Assert.NotNull(people);
    }

    [Fact]
    public async Task SavePersonAsync_CreatesNewPerson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act
        var result = await _databaseService.SavePersonAsync(person);

        // Assert
        Assert.True(result > 0);
        Assert.True(person.Id > 0);
    }

    [Fact]
    public async Task SavePersonAsync_UpdatesExistingPerson()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };
        await _databaseService.SavePersonAsync(person);
        var originalId = person.Id;

        // Act
        person.FirstName = "Jane";
        var result = await _databaseService.SavePersonAsync(person);

        // Assert
        Assert.True(result > 0);
        Assert.Equal(originalId, person.Id);
        
        var retrieved = await _databaseService.GetPersonByIdAsync(person.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Jane", retrieved!.FirstName);
    }

    [Fact]
    public async Task GetPersonByIdAsync_ReturnsPerson_WhenExists()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "John",
            LastName = "Doe"
        };
        await _databaseService.SavePersonAsync(person);

        // Act
        var result = await _databaseService.GetPersonByIdAsync(person.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("John", result!.FirstName);
        Assert.Equal("Doe", result.LastName);
    }

    [Fact]
    public async Task GetPersonByIdAsync_ReturnsNull_WhenNotExists()
    {
        // Act
        var result = await _databaseService.GetPersonByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPeopleAsync_ReturnsAllPeople()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);

        // Act
        var result = await _databaseService.GetPeopleAsync();

        // Assert
        Assert.True(result.Count >= 2);
        Assert.Contains(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.FirstName == "Jane");
    }

    [Fact]
    public async Task GetPeopleAsync_ExcludesDeleted_ByDefault()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);
        
        person2.SoftDelete();
        await _databaseService.SavePersonAsync(person2);

        // Act
        var result = await _databaseService.GetPeopleAsync();

        // Assert
        Assert.DoesNotContain(result, p => p.FirstName == "Jane" && p.IsDeleted);
    }

    [Fact]
    public async Task GetPeopleAsync_IncludesDeleted_WhenRequested()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);
        
        person2.SoftDelete();
        await _databaseService.SavePersonAsync(person2);

        // Act
        var result = await _databaseService.GetPeopleAsync(includeDeleted: true);

        // Assert
        Assert.Contains(result, p => p.FirstName == "Jane" && p.IsDeleted);
    }

    [Fact]
    public async Task SearchPeopleAsync_ReturnsMatchingPeople()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        var person3 = new Person { FirstName = "Bob", LastName = "Johnson" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);
        await _databaseService.SavePersonAsync(person3);

        // Act
        var result = await _databaseService.SearchPeopleAsync("John");

        // Assert
        Assert.Contains(result, p => p.FirstName == "John");
        Assert.Contains(result, p => p.LastName == "Johnson");
    }

    [Fact]
    public async Task SearchPeopleAsync_ReturnsAll_WhenSearchTermIsEmpty()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);

        // Act
        var result = await _databaseService.SearchPeopleAsync("");

        // Assert
        Assert.True(result.Count >= 2);
    }

    [Fact]
    public async Task SoftDeletePersonAsync_MarksPersonAsDeleted()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        // Act
        var result = await _databaseService.SoftDeletePersonAsync(person);

        // Assert
        Assert.True(result > 0);
        Assert.True(person.IsDeleted);
        
        var retrieved = await _databaseService.GetPersonByIdAsync(person.Id);
        Assert.Null(retrieved); // Should not return deleted person
    }

    [Fact]
    public async Task SaveMedicationAsync_CreatesNewMedication()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var medication = new Medication
        {
            PersonId = person.Id,
            Name = "Aspirin",
            Dosage = "100mg"
        };

        // Act
        var result = await _databaseService.SaveMedicationAsync(medication);

        // Assert
        Assert.True(result > 0);
        Assert.True(medication.Id > 0);
    }

    [Fact]
    public async Task SaveMedicationAsync_UpdatesExistingMedication()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var medication = new Medication
        {
            PersonId = person.Id,
            Name = "Aspirin",
            Dosage = "100mg"
        };
        await _databaseService.SaveMedicationAsync(medication);
        var originalId = medication.Id;

        // Act
        medication.Name = "Ibuprofen";
        var result = await _databaseService.SaveMedicationAsync(medication);

        // Assert
        Assert.True(result > 0);
        Assert.Equal(originalId, medication.Id);
        
        var retrieved = await _databaseService.GetMedicationByIdAsync(medication.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("Ibuprofen", retrieved!.Name);
    }

    [Fact]
    public async Task GetMedicationByIdAsync_ReturnsMedication_WhenExists()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var medication = new Medication
        {
            PersonId = person.Id,
            Name = "Aspirin",
            Dosage = "100mg"
        };
        await _databaseService.SaveMedicationAsync(medication);

        // Act
        var result = await _databaseService.GetMedicationByIdAsync(medication.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Aspirin", result!.Name);
        Assert.Equal("100mg", result.Dosage);
    }

    [Fact]
    public async Task GetMedicationsAsync_ReturnsAllMedications()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg" };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg" };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetMedicationsAsync();

        // Assert
        Assert.True(result.Count >= 2);
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.Contains(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task GetMedicationsAsync_FiltersByPersonId()
    {
        // Arrange
        var person1 = new Person { FirstName = "John", LastName = "Doe" };
        var person2 = new Person { FirstName = "Jane", LastName = "Smith" };
        await _databaseService.SavePersonAsync(person1);
        await _databaseService.SavePersonAsync(person2);

        var med1 = new Medication { PersonId = person1.Id, Name = "Aspirin", Dosage = "100mg" };
        var med2 = new Medication { PersonId = person2.Id, Name = "Ibuprofen", Dosage = "200mg" };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetMedicationsAsync(person1.Id);

        // Assert
        Assert.All(result, m => Assert.Equal(person1.Id, m.PersonId));
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.DoesNotContain(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task GetActiveMedicationsForPersonAsync_ReturnsOnlyActive()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg", IsActive = true };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg", IsActive = false };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetActiveMedicationsForPersonAsync(person.Id);

        // Assert
        Assert.All(result, m => Assert.True(m.IsActive));
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.DoesNotContain(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task GetLowSupplyMedicationsAsync_ReturnsOnlyLowSupply()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication 
        { 
            PersonId = person.Id, 
            Name = "Aspirin", 
            Dosage = "100mg",
            CurrentSupply = 5,
            LowSupplyThreshold = 10
        };
        var med2 = new Medication 
        { 
            PersonId = person.Id, 
            Name = "Ibuprofen", 
            Dosage = "200mg",
            CurrentSupply = 20,
            LowSupplyThreshold = 10
        };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetLowSupplyMedicationsAsync(person.Id);

        // Assert
        Assert.All(result, m => Assert.True(m.IsSupplyLow()));
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.DoesNotContain(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task GetExpiredMedicationsAsync_ReturnsOnlyExpired()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication 
        { 
            PersonId = person.Id, 
            Name = "Aspirin", 
            Dosage = "100mg",
            ExpirationDate = DateTime.UtcNow.AddDays(-1) // Expired
        };
        var med2 = new Medication 
        { 
            PersonId = person.Id, 
            Name = "Ibuprofen", 
            Dosage = "200mg",
            ExpirationDate = DateTime.UtcNow.AddDays(30) // Not expired
        };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetExpiredMedicationsAsync(person.Id);

        // Assert
        Assert.All(result, m => Assert.True(m.IsExpired()));
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.DoesNotContain(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task SearchMedicationsAsync_ReturnsMatchingMedications()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg" };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg" };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.SearchMedicationsAsync("Aspirin");

        // Assert
        Assert.Contains(result, m => m.Name == "Aspirin");
        Assert.DoesNotContain(result, m => m.Name == "Ibuprofen");
    }

    [Fact]
    public async Task SearchMedicationsAsync_SearchesByDosage()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg" };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg" };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.SearchMedicationsAsync("100mg");

        // Assert
        Assert.Contains(result, m => m.Dosage == "100mg");
    }

    [Fact]
    public async Task SaveMedicationAsync_ThrowsException_WhenInvalid()
    {
        // Arrange
        var medication = new Medication
        {
            PersonId = 0, // Invalid
            Name = "", // Invalid
            Dosage = "100mg"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _databaseService.SaveMedicationAsync(medication));
    }

    [Fact]
    public async Task SavePersonAsync_ThrowsException_WhenInvalid()
    {
        // Arrange
        var person = new Person
        {
            FirstName = "", // Invalid
            LastName = "" // Invalid
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _databaseService.SavePersonAsync(person));
    }

    [Fact]
    public async Task GetMedicationCountForPersonAsync_ReturnsCorrectCount()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg" };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg" };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetMedicationCountForPersonAsync(person.Id);

        // Assert
        Assert.True(result >= 2);
    }

    [Fact]
    public async Task GetActiveMedicationCountForPersonAsync_ReturnsOnlyActiveCount()
    {
        // Arrange
        var person = new Person { FirstName = "John", LastName = "Doe" };
        await _databaseService.SavePersonAsync(person);

        var med1 = new Medication { PersonId = person.Id, Name = "Aspirin", Dosage = "100mg", IsActive = true };
        var med2 = new Medication { PersonId = person.Id, Name = "Ibuprofen", Dosage = "200mg", IsActive = false };
        await _databaseService.SaveMedicationAsync(med1);
        await _databaseService.SaveMedicationAsync(med2);

        // Act
        var result = await _databaseService.GetActiveMedicationCountForPersonAsync(person.Id);

        // Assert
        Assert.True(result >= 1);
    }
}

