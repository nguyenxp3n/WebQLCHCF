using WebQLCHCF.Services;
using Xunit;

namespace WebQLCHCF.Tests;

public class PayrollPrecisionTests
{
    [Theory]
    [InlineData(0, 25000, 0)]
    [InlineData(-10, 25000, 0)]
    [InlineData(60, 0, 0)]
    [InlineData(60, -25000, 0)]
    public void TinhTienCongTuPhut_ZeroOrNegativeInputs_ReturnsZero(int tongPhut, decimal donGiaGio, decimal expected)
    {
        decimal result = PayrollCalculator.TinhTienCongTuPhut(tongPhut, donGiaGio);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(60, 25000, 25000)]
    [InlineData(120, 30000, 60000)]
    [InlineData(270, 25000, 112500)] // 4h30p = 270 phút
    [InlineData(480, 35000, 280000)] // 8h = 480 phút
    [InlineData(525, 28000, 245000)] // 8h45p = 525 phút
    public void TinhTienCongTuPhut_StandardHourShifts_MatchesExpectedWage(int tongPhut, decimal donGiaGio, decimal expected)
    {
        decimal result = PayrollCalculator.TinhTienCongTuPhut(tongPhut, donGiaGio);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void TinhTienCongTuPhut_CriticalDriftCase_10MinutesAt20000_Eliminates67VndDrift()
    {
        const int tongPhut = 10;
        const decimal donGiaGio = 20000m;

        // Old 2-step calculation with rounding drift:
        // Step 1: ROUND(10 / 60.0, 2) = 0.17h
        // Step 2: ROUND(0.17 * 20000, 0) = 3400đ
        decimal oldTwoStepHours = PayrollCalculator.TinhTongGio(tongPhut); // 0.17m
        decimal oldTwoStepWage = PayrollCalculator.TinhTienCong(oldTwoStepHours, donGiaGio); // 3400m
        Assert.Equal(3400m, oldTwoStepWage);

        // SQL Server Stored Procedure ChamCong_CheckOut exact calculation:
        // CAST(ROUND((CAST(10 AS decimal(18,4)) / 60.0) * CAST(20000 AS decimal(18,4)), 0) AS money) = 3333đ
        decimal sqlExpectedWage = Math.Round((decimal)tongPhut / 60.0m * donGiaGio, 0, MidpointRounding.AwayFromZero);
        Assert.Equal(3333m, sqlExpectedWage);

        // TinhTienCongTuPhut eliminates the 67đ rounding discrepancy:
        decimal actualWage = PayrollCalculator.TinhTienCongTuPhut(tongPhut, donGiaGio);
        Assert.Equal(3333m, actualWage);
        Assert.Equal(sqlExpectedWage, actualWage);
        Assert.NotEqual(oldTwoStepWage, actualWage);
        Assert.Equal(67m, oldTwoStepWage - actualWage);
    }

    [Theory]
    [InlineData(20, 25000, 8333, 8250)]   // SQL: 8333đ vs Old: 0.33h * 25k = 8250đ (diff 83đ)
    [InlineData(40, 25000, 16667, 16750)] // SQL: 16667đ vs Old: 0.67h * 25k = 16750đ (diff 83đ)
    [InlineData(5, 22000, 1833, 1760)]    // SQL: 1833đ vs Old: 0.08h * 22k = 1760đ (diff 73đ)
    [InlineData(17, 27000, 7650, 7560)]   // SQL: 7650đ vs Old: 0.28h * 27k = 7560đ (diff 90đ)
    public void TinhTienCongTuPhut_AdditionalDriftScenarios_MatchesSqlServerPrecision(
        int tongPhut,
        decimal donGiaGio,
        decimal expectedSqlWage,
        decimal oldDriftedWage)
    {
        decimal actualWage = PayrollCalculator.TinhTienCongTuPhut(tongPhut, donGiaGio);

        Assert.Equal(expectedSqlWage, actualWage);

        decimal oldWage = PayrollCalculator.TinhTienCong(PayrollCalculator.TinhTongGio(tongPhut), donGiaGio);
        Assert.Equal(oldDriftedWage, oldWage);
    }

    [Theory]
    [InlineData(20000)]
    [InlineData(25000)]
    [InlineData(30000)]
    [InlineData(35000)]
    public void TinhTienCongTuPhut_ContinuousMinuteRange_ZeroDriftAgainstSqlServerFormula(decimal donGiaGio)
    {
        // Stress-test every minute from 1 minute up to 600 minutes (10 hours)
        for (int minute = 1; minute <= 600; minute++)
        {
            decimal sqlFormula = Math.Round((decimal)minute / 60.0m * donGiaGio, 0, MidpointRounding.AwayFromZero);
            decimal csharpResult = PayrollCalculator.TinhTienCongTuPhut(minute, donGiaGio);

            Assert.Equal(sqlFormula, csharpResult);
        }
    }
}
