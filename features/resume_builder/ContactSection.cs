namespace ResumesBuilder;

public sealed class ContactSection : IResumeSection
{
    private readonly string _email;
    private readonly string _address;
    private readonly string _phoneNumber;
    private readonly string _linkedInProfileUrl;
    public string Email => _email;
    public string Address => _address;
    public string PhoneNumber => _phoneNumber;
    public string LinkedInProfileUrl => _linkedInProfileUrl;

    public ContactSection(
        string email,
        string address,
        string phoneNumber,
        string linkedInProfileUrl
    )
    {
        ArgumentNullException.ThrowIfNull(email, nameof(email));
        ArgumentNullException.ThrowIfNull(address, nameof(address));
        ArgumentNullException.ThrowIfNull(phoneNumber, nameof(phoneNumber));
        ArgumentNullException.ThrowIfNull(linkedInProfileUrl, nameof(linkedInProfileUrl));

        if (!Helpers.IsValidEmail(email))
            throw new ArgumentException("Invalid email.", nameof(email));
        if (!Helpers.IsValidMobileNumber(phoneNumber))
            throw new ArgumentException("Invalid phone number.", nameof(phoneNumber));
        if (!Helpers.IsValidUrl(linkedInProfileUrl))
            throw new ArgumentException("Invalid LinkedIn profile URL.", nameof(linkedInProfileUrl));

        _email = email;
        _address = address;
        _phoneNumber = phoneNumber;
        _linkedInProfileUrl = linkedInProfileUrl;
    }

    public string Title => "Contact Information";
    public bool IsEmpty => string.IsNullOrWhiteSpace(_email) ||
        string.IsNullOrWhiteSpace(_address) ||
        string.IsNullOrWhiteSpace(_phoneNumber) ||
        string.IsNullOrWhiteSpace(_linkedInProfileUrl);
    public string Render() =>
        $"""
        Email: {_email}
        Address: {_address}
        Phone Number: {_phoneNumber}
        LinkedIn Profile URL: {_linkedInProfileUrl}
        """;
    public override string ToString() => Render();
    public override bool Equals(object? obj)
    {
        if (obj is not ContactSection other) return false;
        return _email == other._email &&
            _address == other._address &&
            _phoneNumber == other._phoneNumber &&
            _linkedInProfileUrl == other._linkedInProfileUrl;
    }
    public override int GetHashCode() =>
        HashCode.Combine(_email, _address, _phoneNumber, _linkedInProfileUrl);
}
