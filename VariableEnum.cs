namespace AdoptionAlacrityDashboard
{
    /// <summary>
    /// Columns returned from the [ObservationsByStateYear] View that can be used as independent variables
    /// </summary>
    public enum IndependentVariable
    {
        // the enum values match the combo box index of the enum string in the independent variable on the regression tab of Form1
        // if new values are added to one the need to be updated in the other
        Subsidy = 0,
        Married = 1,
        AverageAge = 2,
        Male = 3,
        Black = 4,
        Hispanic = 5,
        NativeAmerican = 6,
        White = 7,
        NonRelative = 8
    }
}
