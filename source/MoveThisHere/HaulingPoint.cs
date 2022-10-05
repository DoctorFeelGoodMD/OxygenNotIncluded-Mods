using KSerialization;
using System;
using System.Collections.Generic;
using UnityEngine;
using STRINGS;

namespace MoveThisHere
{
    public class HaulingPoint : KMonoBehaviour, ISim1000ms, ISingleSliderControl //, IUserControlledCapacity
    {
		#pragma warning disable CS0649
		#pragma warning disable IDE0044
		[MyCmpGet]
		private Storage storage;
		#pragma warning restore IDE0044
		#pragma warning restore CS0649

		[Serialize]
		public bool allowManualPumpingStationFetching;

		[Serialize]
		private float userMaxCapacity = float.PositiveInfinity;

		[Serialize]
		private bool willSelfDestruct = false;

		[Serialize]
		public bool willSpill = false;

		private Tag[] forbidden_tags;

		public float totalMaxCapacity; 
		//create new float for total max capacity and now using public capacitykg in Storage to hold user capacity which used to be the max
		//this is a clumsy workaround to use a custom slider to hold user capacity, rather than default iusercontrolledcapacity which is null
		//all because I can't get IUserControlledCapacity to allow decimal values, and I know you nerds are gonna wanna store 35g or something

		public string SliderTitleKey => "Maximum Capacity";

		public string SliderUnits => GameUtil.GetCurrentMassUnit();
		public float GetSliderMax(int index)
		{
			return totalMaxCapacity;
		}

		public float GetSliderMin(int index)
		{
			return 0.0f;
		}

		public float GetSliderValue(int index)
		{
			return userMaxCapacity;
		}

		public string GetSliderTooltip()
		{
			return "Maximum mass to bring to this Hauling Point";//string.Format(Strings.Get(GetSliderTooltipKey(0)), userMaxCapacity);
		}

		public string GetSliderTooltipKey(int index)
		{
			return "";
		}
		public void SetSliderValue(float value, int index)
		{
			if (value != userMaxCapacity) //setslidervalue runs each time slider appears AND if changed - check if actually changed to avoid unncessary job interruptions
			{
				if (value > 100f)
				{
					value = (float)Math.Round((decimal)value);
					//will round off decimals above 100kg to avoid weird 5g bits when slider is moved instead of typed number
					//if you really want 200.15kg, use two hauling points
				}
				storage.capacityKg = value;
				userMaxCapacity = value; //set both local and Storage variable, local variable gets kept on save/load
				filteredStorage.FilterChanged();
			}
		}
		public int SliderDecimalPlaces(int index)
		{
			return 3; //UI limitations make less than 1g a pain to implement
		}


		public float AmountStored => storage.MassStored();


		protected override void OnPrefabInit()
		{
			Initialize(use_logic_meter: false);
		}
		

		protected FilteredStorageHaulingPoint filteredStorage;

		public string choreTypeID = Db.Get().ChoreTypes.StorageFetch.Id;

		private static readonly EventSystem.IntraObjectHandler<HaulingPoint> OnCopySettingsDelegate = new EventSystem.IntraObjectHandler<HaulingPoint>(delegate (HaulingPoint component, object data)
		{
			component.OnCopySettings(data);
		});
		private static readonly EventSystem.IntraObjectHandler<HaulingPoint> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<HaulingPoint>(delegate (HaulingPoint component, object data)
		{
			component.OnRefreshUserMenu(data);
		});



		protected void Initialize(bool use_logic_meter)
		{
			//initialize comes first, then spawn
			base.OnPrefabInit();

			ChoreType fetch_chore_type = Db.Get().ChoreTypes.Get(choreTypeID);

			forbidden_tags = (allowManualPumpingStationFetching ? new Tag[0] : new Tag[1] { GameTags.LiquidSource });

			filteredStorage = new FilteredStorageHaulingPoint(this, forbidden_tags, null, use_logic_meter, fetch_chore_type);
			//replacing capacity_control slider in filteredstorage - leave it null and do the logic for it here
			//forbidden tags contains either nothing or pump stations. have to make own copy of filteredstorage just to keep this private field updated

			Subscribe(-905833192, OnCopySettingsDelegate);
			Subscribe(493375141, OnRefreshUserMenuDelegate);


		}

		protected override void OnSpawn()
		{
			base.OnSpawn();

			if (userMaxCapacity >= totalMaxCapacity)
			{
				userMaxCapacity = totalMaxCapacity;
			}
			storage.capacityKg = userMaxCapacity; //set this up since capacitykg isn't serialized, I'm sure there is an easier way but whatever
			//must read serialized variables during onspawn, not initialize, I guess they are not unserialized until now.

			filteredStorage.FilterChanged();

		}
		private void OnChangeAllowManualPumpingStationFetching()
		{
			allowManualPumpingStationFetching = !allowManualPumpingStationFetching;

			forbidden_tags = (allowManualPumpingStationFetching ? new Tag[0] : new Tag[1] { GameTags.LiquidSource });
			filteredStorage.SetForbiddenTags(forbidden_tags);
			filteredStorage.FilterChanged();

		}
		private void OnChangeWillSpill()
		{
			willSpill = !willSpill;

		}
		private void ToggleWillSelfDestruct()
        {
			willSelfDestruct = !willSelfDestruct;
        }

		protected override void OnCleanUp()
		{
			filteredStorage.CleanUp();
		}


		private void OnCopySettings(object data)
		{
			GameObject gameObject = (GameObject)data;
			if (!(gameObject == null))
			{
				HaulingPoint component = gameObject.GetComponent<HaulingPoint>();
				if (!(component == null))
				{
					//this is copying settings TO the local variables from clipboard component
					userMaxCapacity = component.userMaxCapacity;
					storage.capacityKg = userMaxCapacity;
					willSelfDestruct = component.willSelfDestruct;
					willSpill = component.willSpill;
					allowManualPumpingStationFetching = component.allowManualPumpingStationFetching;
					forbidden_tags = (allowManualPumpingStationFetching ? new Tag[0] : new Tag[1] { GameTags.LiquidSource });
					filteredStorage.SetForbiddenTags(forbidden_tags);
					filteredStorage.FilterChanged();

				}
			}
		}

		private void OnRefreshUserMenu(object data)
		{
			KIconButtonMenu.ButtonInfo button2 = (allowManualPumpingStationFetching ? new KIconButtonMenu.ButtonInfo("action_bottler_delivery", UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED.NAME, OnChangeAllowManualPumpingStationFetching, Action.NumActions, null, null, null, UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.DENIED.TOOLTIP) : new KIconButtonMenu.ButtonInfo("action_bottler_delivery", UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED.NAME, OnChangeAllowManualPumpingStationFetching, Action.NumActions, null, null, null, UI.USERMENUACTIONS.MANUAL_PUMP_DELIVERY.ALLOWED.TOOLTIP));
			Game.Instance.userMenu.AddButton(base.gameObject, button2, 0.4f);

			KIconButtonMenu.ButtonInfo button = (willSelfDestruct ? new KIconButtonMenu.ButtonInfo("action_empty_contents", HaulingPointConfig.SelfDestructButtonCancelText, ToggleWillSelfDestruct, Action.NumActions, null, null, null, HaulingPointConfig.SelfDestructButtonCancelTooltip) : new KIconButtonMenu.ButtonInfo("action_empty_contents", HaulingPointConfig.SelfDestructButtonText, ToggleWillSelfDestruct, Action.NumActions, null, null, null, HaulingPointConfig.SelfDestructButtonTooltip));
			Game.Instance.userMenu.AddButton(base.gameObject, button);

			KIconButtonMenu.ButtonInfo button3 = (willSpill ? new KIconButtonMenu.ButtonInfo("action_bottler_delivery", HaulingPointConfig.SpillButtonCancelText, OnChangeWillSpill, Action.NumActions, null, null, null, HaulingPointConfig.SpillButtonCancelTooltip) : new KIconButtonMenu.ButtonInfo("action_bottler_delivery", HaulingPointConfig.SpillButtonText, OnChangeWillSpill, Action.NumActions, null, null, null, HaulingPointConfig.SpillButtonTooltip));
			Game.Instance.userMenu.AddButton(base.gameObject, button3);

		}

		public void Sim1000ms(float dt)
		{
			if (willSelfDestruct) 
			{ 
				if ((AmountStored / userMaxCapacity) >= .99) //give a little wiggle for sublimination, stock margin doesn't work with low mass
				{
					GetComponentInParent<DeconstructableHaulingPoint>().OnDeconstruct();

				}
			}
		}

		}

	public class DeconstructableHaulingPoint : Workable
    {

		//modified deconstructable to replace default behavior, this one will deconstruct instantly when given decon order
		//however it won't drop any resources from the building itself, important because it's made of vacuum and this gives an error
		//also drops gas resource in canister form

		private static readonly EventSystem.IntraObjectHandler<DeconstructableHaulingPoint> OnRefreshUserMenuDelegate = new EventSystem.IntraObjectHandler<DeconstructableHaulingPoint>(delegate (DeconstructableHaulingPoint component, object data)
		{
			component.OnRefreshUserMenu(data);
		});
		private static readonly EventSystem.IntraObjectHandler<DeconstructableHaulingPoint> OnDeconstructDelegate = new EventSystem.IntraObjectHandler<DeconstructableHaulingPoint>(delegate (DeconstructableHaulingPoint component, object data)
		{
			component.OnDeconstruct();
		});
		private CellOffset[] placementOffsets
		{
			get
			{
				Building component = GetComponent<Building>();
				if (component != null)
				{
					return component.Def.PlacementOffsets;
				}

				Debug.Assert(condition: false, "There's some error with MoveThisHere mod that the developer doesn't understand", this);
				return null;

			}
		}

		protected override void OnPrefabInit()
		{
			base.OnPrefabInit();
			Subscribe(493375141, OnRefreshUserMenuDelegate);
			Subscribe(-111137758, OnRefreshUserMenuDelegate);
			Subscribe(-790448070, OnDeconstructDelegate);

			CellOffset[][] table = OffsetGroups.InvertedStandardTable;
			CellOffset[] filter = null;
			CellOffset[][] offsetTable = OffsetGroups.BuildReachabilityTable(placementOffsets, table, filter);
			SetOffsetTable(offsetTable); 
			//I really don't know what this celloffset stuff is about, too afraid to delete
			//from original deconstructable class
			

		}
		protected override void OnSpawn()
		{
			base.OnSpawn();

		}
		public void OnDeconstruct()
		{

			Storage storage = base.GetComponent<Storage>();
			HaulingPoint haulingPoint = base.GetComponent<HaulingPoint>();

			storage.DropAll(haulingPoint.willSpill, haulingPoint.willSpill); //drop liquids and gasses based on setting

			base.gameObject.DeleteObject(); //goodbye
		}


		private void OnRefreshUserMenu(object data)
		{
			if (!this.HasTag(GameTags.Stored))
			{
				KIconButtonMenu.ButtonInfo button = new KIconButtonMenu.ButtonInfo("action_deconstruct", HaulingPointConfig.DeconstructButtonText, OnDeconstruct, Action.NumActions, null, null, null, HaulingPointConfig.DeconstructButtonTooltip) ;//: new KIconButtonMenu.ButtonInfo("action_deconstruct", UI.USERMENUACTIONS.DECONSTRUCT.NAME_OFF, OnDeconstruct, Action.NumActions, null, null, null, UI.USERMENUACTIONS.DECONSTRUCT.TOOLTIP_OFF));
				Game.Instance.userMenu.AddButton(base.gameObject, button, 0f);
				//add deconstruct button
				//I thought about using cancel tool instead, but since it is made through build menu I thought this would be more intuivitive
			}
		}



	}


	public class FilteredStorageHaulingPoint
	{
		//this class is basically a copy of filteredstorage with just a few changes necessary to make hauling points work properly
		//for example, handling of forbidden tags for the auto bottler

		public static readonly HashedString FULL_PORT_ID = "FULL";

		private KMonoBehaviour root;

		private FetchList2 fetchList;

		private IUserControlledCapacity capacityControl;

		private TreeFilterable filterable;

		private Storage storage;

		private MeterController meter;

		private MeterController logicMeter;

		//private Tag[] requiredTags;

		private Tag[] forbiddenTags;

		private bool hasMeter = true;

		private bool useLogicMeter;

		private ChoreType choreType;

		public void SetHasMeter(bool has_meter)
		{
			hasMeter = has_meter;
		}

		public FilteredStorageHaulingPoint(KMonoBehaviour root, Tag[] forbidden_tags, IUserControlledCapacity capacity_control, bool use_logic_meter, ChoreType fetch_chore_type)
		{
			this.root = root;
			forbiddenTags = forbidden_tags;
			capacityControl = capacity_control;
			useLogicMeter = use_logic_meter;
			choreType = fetch_chore_type;
			root.Subscribe(-1697596308, OnStorageChanged);
			root.Subscribe(-543130682, OnUserSettingsChanged);
			filterable = root.FindOrAdd<TreeFilterable>();
			TreeFilterable treeFilterable = filterable;
			treeFilterable.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Combine(treeFilterable.OnFilterChanged, new Action<HashSet<Tag>>(OnFilterChanged));
			storage = root.GetComponent<Storage>();
			storage.Subscribe(644822890, OnOnlyFetchMarkedItemsSettingChanged);
			storage.Subscribe(-1852328367, OnFunctionalChanged);
		}

		private void OnOnlyFetchMarkedItemsSettingChanged(object data)
		{
			OnFilterChanged(filterable.GetTags());
		}

		private void CreateMeter()
		{
			if (hasMeter)
			{
				meter = new MeterController(root.GetComponent<KBatchedAnimController>(), "meter_target", "meter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer, "meter_frame", "meter_level");
			}
		}

		private void CreateLogicMeter()
		{
			if (hasMeter)
			{
				logicMeter = new MeterController(root.GetComponent<KBatchedAnimController>(), "logicmeter_target", "logicmeter", Meter.Offset.Infront, Grid.SceneLayer.NoLayer);
			}
		}

		public void CleanUp()
		{
			if (filterable != null)
			{
				TreeFilterable treeFilterable = filterable;
				treeFilterable.OnFilterChanged = (Action<HashSet<Tag>>)Delegate.Remove(treeFilterable.OnFilterChanged, new Action<HashSet<Tag>>(OnFilterChanged));
			}
			if (fetchList != null)
			{
				fetchList.Cancel("Parent destroyed");
			}
		}

		public void FilterChanged()
		{
			if (hasMeter)
			{
				if (meter == null)
				{
					CreateMeter();
				}
				if (logicMeter == null && useLogicMeter)
				{
					CreateLogicMeter();
				}
			}
			OnFilterChanged(filterable.GetTags());
			UpdateMeter();
		}

		private void OnUserSettingsChanged(object data)
		{
			OnFilterChanged(filterable.GetTags());
			UpdateMeter();
		}

		private void OnStorageChanged(object data)
		{
			if (fetchList == null)
			{
				OnFilterChanged(filterable.GetTags());
			}
			UpdateMeter();
		}

		private void OnFunctionalChanged(object data)
		{
			OnFilterChanged(filterable.GetTags());
		}

		private void UpdateMeter()
		{
			float maxCapacityMinusStorageMargin = GetMaxCapacityMinusStorageMargin();
			float positionPercent = Mathf.Clamp01(GetAmountStored() / maxCapacityMinusStorageMargin);
			if (meter != null)
			{
				meter.SetPositionPercent(positionPercent);
			}
		}

		public bool IsFull()
		{
			float maxCapacityMinusStorageMargin = GetMaxCapacityMinusStorageMargin();
			float num = Mathf.Clamp01(GetAmountStored() / maxCapacityMinusStorageMargin);
			if (meter != null)
			{
				meter.SetPositionPercent(num);
			}
			if (!(num >= 1f))
			{
				return false;
			}
			return true;
		}

		private void OnFetchComplete()
		{
			OnFilterChanged(filterable.GetTags());
		}

		private float GetMaxCapacity()
		{
			float num = storage.capacityKg;
			if (capacityControl != null)
			{
				num = Mathf.Min(num, capacityControl.UserMaxCapacity);
			}
			return num;
		}

		private float GetMaxCapacityMinusStorageMargin()
		{
			return GetMaxCapacity() - storage.storageFullMargin;
		}

		private float GetAmountStored()
		{
			float result = storage.MassStored();
			if (capacityControl != null)
			{
				result = capacityControl.AmountStored;
			}
			return result;
		}

		private bool IsFunctional()
		{
			Operational component = storage.GetComponent<Operational>();
			if (!(component == null))
			{
				return component.IsFunctional;
			}
			return true;
		}

		public void SetForbiddenTags(Tag[] forbidden_tags)
        {
			forbiddenTags = forbidden_tags; //wouldn't need this whole class except for that
			//and actually, after the update 10/4 which added new public methods to modify forbidden tags, may well be entirely unnecessary
			//but... if it ain't broke, I'm not fixing it
        }

		private void OnFilterChanged(HashSet<Tag> tags)
		{
			bool flag = tags != null && tags.Count != 0;
			if (fetchList != null)
			{
				fetchList.Cancel("");
				fetchList = null;
			}
			float maxCapacityMinusStorageMargin = GetMaxCapacityMinusStorageMargin();
			float amountStored = GetAmountStored();
			float num = Mathf.Max(0f, maxCapacityMinusStorageMargin - amountStored);
			if (num > 0f && flag && IsFunctional())
			{
				num = Mathf.Max(0f, GetMaxCapacity() - amountStored);
				fetchList = new FetchList2(storage, choreType);
				fetchList.ShowStatusItem = false;
				fetchList.Add(tags, forbiddenTags, num, Operational.State.Functional);
				fetchList.Submit(OnFetchComplete, check_storage_contents: false);
			}
		}

		public void SetLogicMeter(bool on)
		{
			if (logicMeter != null)
			{
				logicMeter.SetPositionPercent(on ? 1f : 0f);
			}
		}
		/*public void AddForbiddenTag(Tag forbidden_tag)
		{
			if (forbiddenTags == null)
			{
				forbiddenTags = new Tag[0];
			}
			if (!forbiddenTags.Contains(forbidden_tag))
			{
				forbiddenTags = forbiddenTags.Append(forbidden_tag);
				OnFilterChanged(filterable.GetTags());
			}
		}

		public void RemoveForbiddenTag(Tag forbidden_tag)
		{
			if (forbiddenTags != null)
			{
				List<Tag> list = new List<Tag>(forbiddenTags);
				list.Remove(forbidden_tag);
				forbiddenTags = list.ToArray();
				OnFilterChanged(filterable.GetTags());
			}
		}*/
	}


}