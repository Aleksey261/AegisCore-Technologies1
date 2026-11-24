using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace AegisCore.Services.ML;

public interface IModelProvider
{
    string ModelName { get; }
    string Description { get; }
    ITransformer Train(MLContext context, IDataView trainingData);
}

// Единый набор признаков для всех моделей — честное сравнение
internal static class FeaturePipeline
{
    public static IEstimator<ITransformer> Build(MLContext context) =>
        context.Transforms.Concatenate("Features",
            nameof(SecurityData.PortOpen),
            nameof(SecurityData.WeakPassword),
            nameof(SecurityData.OutdatedSoftware),
            nameof(SecurityData.UnencryptedTraffic),
            nameof(SecurityData.DestinationPort),
            nameof(SecurityData.FlowDuration),
            nameof(SecurityData.TotalFwdPackets),
            nameof(SecurityData.TotalBackwardPackets),
            nameof(SecurityData.TotalLengthFwdPackets),
            nameof(SecurityData.TotalLengthBwdPackets),
            nameof(SecurityData.FwdPacketLengthMean),
            nameof(SecurityData.BwdPacketLengthMean),
            nameof(SecurityData.FlowBytesPerSec),
            nameof(SecurityData.FlowPacketsPerSec),
            nameof(SecurityData.FlowIatMean),
            nameof(SecurityData.PacketLengthMean),
            nameof(SecurityData.PacketLengthStd),
            nameof(SecurityData.SynFlagCount),
            nameof(SecurityData.AckFlagCount),
            nameof(SecurityData.AveragePacketSize),
            nameof(SecurityData.InitWinBytesForward),
            nameof(SecurityData.InitWinBytesBackward))
        .Append(context.Transforms.NormalizeMinMax("Features")); // важно для SVM/SDCA/перцептрона
}

// 1. LightGBM
public class LightGbmProvider : IModelProvider
{
    public string ModelName => "LightGBM";
    public string Description => "Градиентный бустинг (LightGBM).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.LightGbm(
                labelColumnName: nameof(SecurityData.Label),
                numberOfLeaves: 15, learningRate: 0.1f, minimumExampleCountPerLeaf: 10))
            .Fit(trainingData);
}

// 2. Random Forest (FastForest)
public class RandomForestProvider : IModelProvider
{
    public string ModelName => "Random Forest";
    public string Description => "Случайный лес (FastForest).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.FastForest(
                labelColumnName: nameof(SecurityData.Label),
                numberOfTrees: 100, minimumExampleCountPerLeaf: 5))
            .Fit(trainingData);
}

// 3. SVM — теперь настоящий (LinearSvm), а не SDCA
public class SvmProvider : IModelProvider
{
    public string ModelName => "SVM";
    public string Description => "Метод опорных векторов (LinearSvm).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.LinearSvm(
                labelColumnName: nameof(SecurityData.Label),
                numberOfIterations: 100))
            .Fit(trainingData);
}

// 4. FastTree
public class FastTreeProvider : IModelProvider
{
    public string ModelName => "FastTree";
    public string Description => "Градиентный бустинг (FastTree).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(SecurityData.Label),
                numberOfLeaves: 15, learningRate: 0.1f, minimumExampleCountPerLeaf: 10))
            .Fit(trainingData);
}

// 5. Бывший "Isolation Forest" → честная логистическая регрессия (SDCA).
//    Isolation Forest в ML.NET нет; имя класса оставлено для совместимости с MLPipeline.
public class IsolationForestProvider : IModelProvider
{
    public string ModelName => "Logistic Regression (SDCA)";
    public string Description => "Линейная логистическая регрессия (SDCA).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(SecurityData.Label),
                maximumNumberOfIterations: 100, l2Regularization: 0.01f))
            .Fit(trainingData);
}

// 6. Бывший "Autoencoders" → L-BFGS логистическая регрессия
public class AutoencoderProvider : IModelProvider
{
    public string ModelName => "Logistic Regression (L-BFGS)";
    public string Description => "Логистическая регрессия, оптимизация L-BFGS.";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.LbfgsLogisticRegression(
                labelColumnName: nameof(SecurityData.Label),
                l2Regularization: 0.01f))
            .Fit(trainingData);
}

// 7. Бывший "RNN/LSTM" → усреднённый перцептрон
public class RnnLstmProvider : IModelProvider
{
    public string ModelName => "Averaged Perceptron";
    public string Description => "Линейный классификатор (усреднённый перцептрон).";
    public ITransformer Train(MLContext context, IDataView trainingData) =>
        FeaturePipeline.Build(context)
            .Append(context.BinaryClassification.Trainers.AveragedPerceptron(
                labelColumnName: nameof(SecurityData.Label),
                numberOfIterations: 100))
            .Fit(trainingData);
}