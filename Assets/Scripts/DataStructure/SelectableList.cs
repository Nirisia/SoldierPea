using System.Collections;
using System.Collections.Generic;

/* simple wraper to encapsulate a list of selectable entity */
public class SelectableList<T> : IEnumerable<T>  where T : ISelectable 
{
	private List<T> _data = new List<T>();

	/*========= Accessors =========*/
	public int		Count => _data.Count;
	public List<T>	List => _data;

	/*========= Add/Remove =========*/

	public void Add(T selectable_)
	{
		selectable_.SetSelected(true);
		_data.Add(selectable_);
	}

	public void Remove(T selectable_)
	{
		selectable_.SetSelected(false);
		_data.Remove(selectable_);
	}

	/*========= Add/Remove List/Array =========*/

	public void AddRange(List<T> selectables_)
	{
		foreach (ISelectable selectable in selectables_)
			selectable.SetSelected(true);
		_data.AddRange(selectables_);
	}

	public void AddRange(T[] selectables_)
	{
		foreach (ISelectable selectable in selectables_)
			selectable.SetSelected(true);
		_data.AddRange(selectables_);
	}

	/*========= Clear =========*/
	public void Clear()
	{
		foreach (ISelectable selectable in _data)
			selectable.SetSelected(false);
		_data.Clear();
	}

	/*========= operator =========*/

	/* conversion operator from List<ISelectable> */
	public static implicit operator SelectableList<T>(List<T> selectables_)
	{
		SelectableList<T> selectableList = new SelectableList<T>();
		selectableList.AddRange(selectables_);
		return selectableList;
	}

	/* conversion operator from ISelectable[] */
	public static implicit operator SelectableList<T>(T[] selectables_)
	{
		SelectableList<T> selectableList = new SelectableList<T>();
		selectableList.AddRange(selectables_);
		return selectableList;
	}

	/*========= Conversion =========*/

	public T[] ToArray()
	{
		return _data.ToArray();
	}

	/*========= Enumerable =========*/

	public IEnumerator<T> GetEnumerator()
	{
		return _data.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _data.GetEnumerator();
	}
}