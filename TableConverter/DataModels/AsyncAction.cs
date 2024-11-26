using System.Threading.Tasks;

namespace TableConverter.DataModels;

public delegate Task AsyncAction();

public delegate Task AsyncAction<in T>(T obj);