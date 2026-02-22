using System.Data;
using BenchmarkDotNet.Attributes;
using NativeData.Abstractions;
using NativeData.Generated;

namespace NativeData.Benchmarks;

[MemoryDiagnoser]
public class MappingBenchmarks
{
    private readonly BenchEntity _entity = new(42, "native", 99.5m);
    private readonly IDataRecord _record = new DictionaryDataRecord(new Dictionary<string, object?>
    {
        ["Id"] = 42L,
        ["Name"] = "native",
        ["Score"] = 99.5m,
    });

    private readonly IEntityMap<BenchEntity> _generatedMap = NativeDataEntityMaps.Create<BenchEntity>();
    private readonly IEntityMap<BenchEntity> _manualMap = new ManualBenchEntityMap();

    [Benchmark]
    public IReadOnlyList<SqlParameterValue> Generated_InsertParameters()
    {
        return _generatedMap.BuildInsertParameters(_entity);
    }

    [Benchmark]
    public IReadOnlyList<SqlParameterValue> Manual_InsertParameters()
    {
        return _manualMap.BuildInsertParameters(_entity);
    }

    [Benchmark]
    public IReadOnlyList<SqlParameterValue> Generated_UpdateParameters()
    {
        return _generatedMap.BuildUpdateParameters(_entity);
    }

    [Benchmark]
    public IReadOnlyList<SqlParameterValue> Manual_UpdateParameters()
    {
        return _manualMap.BuildUpdateParameters(_entity);
    }

    [Benchmark]
    public BenchEntity Generated_Materialize()
    {
        return _generatedMap.Materialize(_record);
    }

    [Benchmark]
    public BenchEntity Manual_Materialize()
    {
        return _manualMap.Materialize(_record);
    }

    [NativeDataEntity("BenchEntities", "Id")]
    public sealed record BenchEntity(int Id, string Name, decimal Score);

    private sealed class ManualBenchEntityMap : IEntityMap<BenchEntity>
    {
        public string TableName => "BenchEntities";

        public string KeyColumn => "Id";

        public IReadOnlyList<string> WritableColumns => ["Id", "Name", "Score"];

        public object? GetKey(BenchEntity entity) => entity.Id;

        public IReadOnlyList<SqlParameterValue> BuildInsertParameters(BenchEntity entity)
            => [new("Id", entity.Id), new("Name", entity.Name), new("Score", entity.Score)];

        public IReadOnlyList<SqlParameterValue> BuildUpdateParameters(BenchEntity entity)
            => [new("Id", entity.Id), new("Name", entity.Name), new("Score", entity.Score)];

        public BenchEntity Materialize(IDataRecord record)
            => new(Convert.ToInt32(record["Id"]), (string)record["Name"], Convert.ToDecimal(record["Score"]));
    }

    private sealed class DictionaryDataRecord : IDataRecord
    {
        private readonly IReadOnlyList<string> _orderedKeys;
        private readonly IReadOnlyDictionary<string, object?> _values;

        public DictionaryDataRecord(IReadOnlyDictionary<string, object?> values)
        {
            _values = values;
            _orderedKeys = values.Keys.ToArray();
        }

        public object this[int i] => _values[_orderedKeys[i]] ?? DBNull.Value;

        public object this[string name] => _values[name] ?? DBNull.Value;

        public int FieldCount => _orderedKeys.Count;

        public bool GetBoolean(int i) => (bool)this[i];

        public byte GetByte(int i) => (byte)this[i];

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();

        public char GetChar(int i) => (char)this[i];

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();

        public IDataReader GetData(int i) => throw new NotSupportedException();

        public string GetDataTypeName(int i) => GetFieldType(i).Name;

        public DateTime GetDateTime(int i) => (DateTime)this[i];

        public decimal GetDecimal(int i) => Convert.ToDecimal(this[i]);

        public double GetDouble(int i) => Convert.ToDouble(this[i]);

        public Type GetFieldType(int i)
        {
            var value = this[i];
            return value is DBNull ? typeof(object) : value.GetType();
        }

        public float GetFloat(int i) => Convert.ToSingle(this[i]);

        public Guid GetGuid(int i) => (Guid)this[i];

        public short GetInt16(int i) => Convert.ToInt16(this[i]);

        public int GetInt32(int i) => Convert.ToInt32(this[i]);

        public long GetInt64(int i) => Convert.ToInt64(this[i]);

        public string GetName(int i) => _orderedKeys[i];

        public int GetOrdinal(string name)
        {
            for (var index = 0; index < _orderedKeys.Count; index++)
            {
                if (string.Equals(_orderedKeys[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }
            }

            return -1;
        }

        public string GetString(int i) => (string)this[i];

        public object GetValue(int i) => this[i];

        public int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, _orderedKeys.Count);
            for (var index = 0; index < count; index++)
            {
                values[index] = this[index];
            }

            return count;
        }

        public bool IsDBNull(int i) => this[i] is DBNull;
    }
}
