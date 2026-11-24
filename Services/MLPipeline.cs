using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.ML;
using Microsoft.ML.Data;
using AegisCore.Services.ML;

namespace AegisCore.Services;

public class SecurityData
{
    public float PortOpen { get; set; }
    public float WeakPassword { get; set; }
    public float OutdatedSoftware { get; set; }
    public float UnencryptedTraffic { get; set; }
    public bool Label { get; set; } // Строго bool для бинарной классификации в ML.NET
}

public class SecurityPrediction
{
    public bool PredictedLabel { get; set; }
    public float Score { get; set; }
}

public class ModelMetrics
{
    public string ModelName { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double Auc { get; set; }

    // НОВЫЕ ПОЛЯ ДЛЯ ДОКАЗАТЕЛЬСТВА РЕАЛЬНОГО ОБУЧЕНИЯ
    public int TruePositives { get; set; }
    public int TrueNegatives { get; set; }
    public int FalsePositives { get; set; }
    public int FalseNegatives { get; set; }
    public int TotalEvaluatedRecords { get; set; }
    public double TrainingTimeMs { get; set; } // Время обучения в мс
}

public class MLPipeline
{
    private readonly MLContext _mlContext;
    private readonly Dictionary<string, IModelProvider> _modelProviders;
    private readonly Dictionary<string, ITransformer> _trainedModels;
    private readonly Dictionary<string, ModelMetrics> _modelMetrics;

    public MLPipeline()
    {
        _mlContext = new MLContext(seed: 42);
        _trainedModels = new Dictionary<string, ITransformer>();
        _modelMetrics = new Dictionary<string, ModelMetrics>();
        
        _modelProviders = new Dictionary<string, IModelProvider>
        {
            ["LightGBM"] = new LightGbmProvider(),
            ["RandomForest"] = new RandomForestProvider(),
            ["SVM"] = new SvmProvider(),
            ["FastTree"] = new FastTreeProvider(),
            ["IsolationForest"] = new IsolationForestProvider(),
            ["Autoencoders"] = new AutoencoderProvider(),
            ["RnnLstm"] = new RnnLstmProvider()
        };
    }

    public IReadOnlyDictionary<string, IModelProvider> GetAvailableModels() => _modelProviders;

    public ModelMetrics TrainModel(string modelName, List<SecurityData> trainingData)
    {
        if (!_modelProviders.TryGetValue(modelName, out var provider))
            throw new ArgumentException($"Модель '{modelName}' не найдена");

        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Начало обучения модели: {modelName}...");
        var stopwatch = Stopwatch.StartNew();

        var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);
        var trainTestSplit = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        var model = provider.Train(_mlContext, trainTestSplit.TrainSet);
        _trainedModels[modelName] = model;

        var testPredictions = model.Transform(trainTestSplit.TestSet);
        
        // Явно указываем имена колонок, чтобы избежать ошибки 
        // "Probability column not found" для моделей типа FastForest (Random Forest)
        var metrics = _mlContext.BinaryClassification.Evaluate(
            testPredictions,
            labelColumnName: nameof(SecurityData.Label),
            scoreColumnName: nameof(SecurityPrediction.Score),
            predictedLabelColumnName: nameof(SecurityPrediction.PredictedLabel),
            probabilityColumnName: nameof(SecurityPrediction.Score) // Мапим вероятность на Score
        );

        stopwatch.Stop();
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Модель {modelName} обучена за {stopwatch.ElapsedMilliseconds} мс.");

        // Извлекаем матрицу ошибок для доказательства реальной оценки
        var cm = metrics.ConfusionMatrix;

        // Counts[фактический класс][предсказанный класс]:
        // [0][0] = TN, [0][1] = FP, [1][0] = FN, [1][1] = TP
        double tp = cm.Counts[1][1];
        double tn = cm.Counts[0][0];
        double fp = cm.Counts[0][1];
        double fn = cm.Counts[1][0];

        var modelMetrics = new ModelMetrics
        {
            ModelName = provider.ModelName,
            Accuracy = metrics.Accuracy,
            Precision = metrics.PositivePrecision,
            Recall = metrics.PositiveRecall,
            F1Score = metrics.F1Score,
            Auc = metrics.AreaUnderRocCurve,

            // Заполняем поля доказательства обучения
            TruePositives = (int)tp,
            TrueNegatives = (int)tn,
            FalsePositives = (int)fp,
            FalseNegatives = (int)fn,
            TotalEvaluatedRecords = (int)(tp + tn + fp + fn),
            TrainingTimeMs = stopwatch.ElapsedMilliseconds
        };

        _modelMetrics[modelName] = modelMetrics;
        return modelMetrics;
    }

    public Dictionary<string, ModelMetrics> TrainAllModels(List<SecurityData> trainingData)
    {
        var results = new Dictionary<string, ModelMetrics>();
        
        foreach (var modelName in _modelProviders.Keys)
        {
            try
            {
                var metrics = TrainModel(modelName, trainingData);
                results[modelName] = metrics;
            }
            catch (Exception ex)
            {
                // Теперь мы увидим ошибку в консоли, если модель не сможет обучиться
                Console.WriteLine($"❌ ОШИБКА при обучении модели {modelName}: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
                results[modelName] = new ModelMetrics
                {
                    ModelName = modelName,
                    Accuracy = 0,
                    Precision = 0,
                    Recall = 0,
                    F1Score = 0,
                    Auc = 0,
                    TruePositives = 0,
                    TrueNegatives = 0,
                    FalsePositives = 0,
                    FalseNegatives = 0,
                    TotalEvaluatedRecords = 0,
                    TrainingTimeMs = 0
                };
            }
        }

        return results;
    }

    public PredictionEngine<SecurityData, SecurityPrediction> CreatePredictionEngine(string modelName)
    {
        if (!_trainedModels.TryGetValue(modelName, out var model))
            throw new InvalidOperationException($"Модель '{modelName}' не обучена");

        return _mlContext.Model.CreatePredictionEngine<SecurityData, SecurityPrediction>(model);
    }

    public ModelMetrics? GetMetrics(string modelName)
    {
        return _modelMetrics.TryGetValue(modelName, out var metrics) ? metrics : null;
    }

    public IReadOnlyDictionary<string, ModelMetrics> GetAllMetrics() => _modelMetrics;
}