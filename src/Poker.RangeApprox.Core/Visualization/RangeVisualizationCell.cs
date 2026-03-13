namespace Poker.RangeApprox.Core.Visualization;

public sealed record RangeVisualizationCell(
    string Hand,
    int Row,
    int Column,
    string ActionId,
    string ActionLabel,
    double TotalEv,
    double? FoldBranchEv,
    double? CallBranchEv,
    double? ThreeBetBranchEv,
    double? FourBetBranchEv
);