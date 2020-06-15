namespace Framework.Interfaces
{
	interface ISavable<T>
	{
		void Save(string name);
		T Load(string name);
	}
}
