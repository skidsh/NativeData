### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
ND0001 | NativeData.Compatibility | Warning | Avoid runtime type loading via Type.GetType for AOT/trimming safety
ND0002 | NativeData.Compatibility | Warning | Avoid runtime assembly loading via Assembly.Load(string) for AOT/trimming safety
ND0003 | NativeData.Compatibility | Warning | Avoid string-based runtime activation via Activator.CreateInstance(string, string)
ND1001 | NativeData.Mapping | Warning | NativeDataEntity key column must map to a public readable property
ND1002 | NativeData.Mapping | Warning | NativeDataEntity tableName/keyColumn literals must be non-empty and non-whitespace
