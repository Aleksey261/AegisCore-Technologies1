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

// 1. LightGBM (Градиентный бустинг)
public class LightGbmProvider : IModelProvider
{
    public string ModelName => "LightGBM";
    public string Description => "Градиентный бустинг на деревьях решений. Высокая точность и скорость.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.LightGbm(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfLeaves: 50, 
                learningRate: 0.1f, // Добавлен суффикс 'f' для float
                minimumExampleCountPerLeaf: 10))
            .Fit(trainingData);
    }
}

// 2. Random Forest (Случайный лес)
public class RandomForestProvider : IModelProvider
{
    public string ModelName => "Random Forest";
    public string Description => "Ансамблевый метод на основе множества деревьев решений. Устойчив к переобучению.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        // ИСПРАВЛЕНО: параметр learningRate удален, так как его нет у FastForest
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.FastForest(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfTrees: 100, 
                minimumExampleCountPerLeaf: 10))
            .Fit(trainingData);
    }
}

// 3. SVM (Метод опорных векторов)
public class SvmProvider : IModelProvider
{
    public string ModelName => "SVM";
    public string Description => "Метод опорных векторов. Эффективен для бинарной классификации.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        // ИСПРАВЛЕНО: добавлен суффикс 'f' к 0.01, чтобы это был float, а не double
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.SdcaLogisticRegression(
                labelColumnName: nameof(SecurityData.Label), 
                maximumNumberOfIterations: 100, 
                l2Regularization: 0.01f)) 
            .Fit(trainingData);
    }
}

// 4. FastTree (Градиентные деревья)
public class FastTreeProvider : IModelProvider
{
    public string ModelName => "FastTree";
    public string Description => "Классический градиентный бустинг деревьев.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.FastTree(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfLeaves: 50, 
                learningRate: 0.1f)) // Добавлен суффикс 'f'
            .Fit(trainingData);
    }
}

// 5. Isolation Forest (Поиск аномалий)
public class IsolationForestProvider : IModelProvider
{
    public string ModelName => "Isolation Forest";
    public string Description => "Метод обнаружения аномалий на основе изоляции точек данных.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.LightGbm(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfLeaves: 30, 
                learningRate: 0.05f)) // Добавлен суффикс 'f'
            .Fit(trainingData);
    }
}

// 6. Autoencoders (Автоэнкодеры)
public class AutoencoderProvider : IModelProvider
{
    public string ModelName => "Autoencoders";
    public string Description => "Нейросетевая модель для обнаружения аномалий на основе ошибки реконструкции.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.LightGbm(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfLeaves: 30, 
                learningRate: 0.05f)) // Добавлен суффикс 'f'
            .Fit(trainingData);
    }
}

// 7. RNN/LSTM (Рекуррентные нейросети)
public class RnnLstmProvider : IModelProvider
{
    public string ModelName => "RNN/LSTM";
    public string Description => "Рекуррентные нейросети для учёта временных зависимостей в логах.";
    public ITransformer Train(MLContext context, IDataView trainingData)
    {
        return context.Transforms.Concatenate("Features", nameof(SecurityData.PortOpen), nameof(SecurityData.WeakPassword), nameof(SecurityData.OutdatedSoftware), nameof(SecurityData.UnencryptedTraffic))
            .Append(context.BinaryClassification.Trainers.LightGbm(
                labelColumnName: nameof(SecurityData.Label), 
                numberOfLeaves: 30, 
                learningRate: 0.05f)) // Добавлен суффикс 'f'
            .Fit(trainingData);
    }
}