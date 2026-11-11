using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NightBufe.Core;
using NightBufe.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NightBufe.Gameplay
{
    /// <summary>
    /// Main gameplay of the Counter scene: runs the night clock, brings customers,
    /// lets the player build and serve dürüm, and routes choices into GameState.
    /// All rules live in NightBufe.Core; this class only connects them to the scene.
    /// </summary>
    public class CounterController : MonoBehaviour
    {
        public GameDatabase database;

        [Header("Görünümler")]
        public HudView hud;
        public CustomerView customerView;
        public PlateView plateView;
        public RulePaperView rulePaper;
        public RadioController radio;
        public TensionController tension;
        public DonerView doner;
        public FadingText toast;
        public FadingText whisper;
        public DevOverlay devOverlay;

        [Header("Tezgâh")]
        public List<IngredientButton> ingredientButtons = new List<IngredientButton>();
        public WorldButton rulePaperButton;
        public Button wrapButton;
        public Button trashButton;
        public Button serveButton;
        public Button refuseButton;

        [Header("Ayarlar")]
        public float leaveDelaySeconds = 3f;
        public float endDelaySeconds = 3f;

        private GameState _state;
        private NightData _night;
        private NightClock _clock;
        private CustomerQueue _queue;
        private RadioWhisperTracker _whisper;
        private readonly DurumBuilder _plate = new DurumBuilder();
        private readonly HashSet<string> _usedTalkOptions = new HashSet<string>();
        private float[] _anomalyProgress;
        private bool[] _anomalyFired;

        private CustomerData _current;
        private bool _leaving;
        private float _leaveTimer;
        private int _moneyAtStart;
        private bool _ended;

        private void Start()
        {
            GameSession.Init(database);
            _state = GameSession.State;
            _night = database.GetNight(_state.night) ?? database.GetNight(1);
            if (_night == null)
            {
                Debug.LogError("[Counter] Gece verisi bulunamadı. 'Gece Büfesi > 2. Gece 1 Verisini Oluştur' komutunu çalıştır.");
                enabled = false;
                return;
            }

            _moneyAtStart = _state.money;
            _state.nightProgress = 0f;
            _clock = new NightClock(_night.durationSeconds);
            _queue = new CustomerQueue(_night.customers.Select(c => _clock.ProgressAt(c.arrivalTime)).ToList());
            _whisper = new RadioWhisperTracker(_clock.ProgressAt(_night.whisperTime),
                (float)_night.whisperWindowMinutes / _clock.TotalGameMinutes);
            _anomalyProgress = _night.anomalies.Select(a => _clock.ProgressAt(a.time)).ToArray();
            _anomalyFired = new bool[_night.anomalies.Count];

            if (tension != null)
            {
                tension.SetBase(_night.baseTension);
                if (database.art != null)
                {
                    tension.warmColor = database.art.lightWarm;
                    tension.tenseColor = database.art.lightTense;
                }
            }

            WireEvents();

            hud.SetTitle(_night.title);
            hud.SetMoney(_state.money);
            hud.SetClock(0f, _clock.FormatTime());
            plateView.Refresh(_plate);
            SetActionButtons(false);
            toast.Show(_night.introText, 4.5f);

            if (devOverlay != null)
                devOverlay.extraInfo = () =>
                    $"Saat {_clock.FormatTime()}  Müşteri {_queue.NextIndex}/{_queue.Count}  " +
                    $"Radyo {(radio.IsOn ? "açık" : "kapalı")}  Fısıltı {(_whisper.HasFired ? "duyuldu" : _whisper.IsMissed(_clock.Progress) ? "kaçtı" : "bekliyor")}  " +
                    $"Gerilim {(tension != null ? tension.Value : 0f):0.00}";
        }

        private void WireEvents()
        {
            foreach (var button in ingredientButtons)
                if (button != null) button.Picked += OnIngredientPicked;

            if (wrapButton != null) wrapButton.onClick.AddListener(OnWrap);
            if (trashButton != null) trashButton.onClick.AddListener(OnTrash);
            if (rulePaperButton != null) rulePaperButton.Clicked += OpenRulePaper;
            if (serveButton != null) serveButton.onClick.AddListener(OnServe);
            if (refuseButton != null) refuseButton.onClick.AddListener(OnRefuse);

            rulePaper.Signed += OnPaperSigned;
            rulePaper.Flipped += OnPaperFlipped;
        }

        private void Update()
        {
            if (_ended || _clock == null) return;

            bool finished = _clock.Advance(Time.deltaTime * GameSession.DevTimeScale);
            _state.nightProgress = _clock.Progress;
            hud.SetClock(_clock.Progress, _clock.FormatTime());

            if (radio != null && _whisper.Update(radio.IsOn, _clock.Progress))
                OnWhisperHeard();

            UpdateAnomalies();

            if (_leaving)
            {
                _leaveTimer -= Time.deltaTime;
                if (_leaveTimer <= 0f) FinishCustomer();
            }
            else if (_current == null)
            {
                int index = _queue.TryDequeue(_clock.Progress, false);
                if (index >= 0) Arrive(_night.customers[index]);
            }

            if (finished) StartCoroutine(EndNight());
        }

        // ---------- Customers ----------

        private void Arrive(CustomerData customer)
        {
            _current = customer;
            _leaving = false;
            customerView.Show(customer, customer.paleness);
            string greeting = string.IsNullOrEmpty(customer.greeting) ? "" : customer.greeting + "\n";
            customerView.Say(greeting + customer.orderText, OrderHint(customer.order));
            RefreshTalkOptions();
            SetActionButtons(true);
        }

        private void RefreshTalkOptions()
        {
            var customer = _current;
            customerView.ShowTalkOptions(customer.talkOptions,
                i => !_usedTalkOptions.Contains(TalkKey(customer, i)),
                OnTalkPicked);
        }

        private void OnTalkPicked(int index)
        {
            if (_current == null || _leaving) return;
            var option = _current.talkOptions[index];
            _usedTalkOptions.Add(TalkKey(_current, index));
            customerView.Say(option.reply, OrderHint(_current.order));
            ApplyChoice(option.choice);
            RefreshTalkOptions();
        }

        private static string TalkKey(CustomerData customer, int index) => customer.id + "#" + index;

        private void OnServe()
        {
            if (_current == null || _leaving)
            {
                toast.Show("Servis edecek kimse yok.", 1.5f);
                return;
            }

            var served = _current.servedChoice;
            var result = ServiceResolver.Serve(_state, _current.order, _plate, _current.price, _current.paysMoney,
                served != null ? served.id : null, served != null ? served.effects : null);

            switch (result.outcome)
            {
                case ServeOutcome.Empty:
                    toast.Show("Tabak boş.", 1.5f);
                    return;
                case ServeOutcome.Wrong:
                    toast.Show("Yanlış sipariş. Para yok.", 2f);
                    Leave(_current.wrongOrderLine);
                    break;
                case ServeOutcome.Correct:
                    if (result.moneyEarned > 0) toast.Show($"+{result.moneyEarned} TL", 1.5f);
                    if (result.choiceApplied) ShowChoiceResult(served);
                    Leave(_current.GetServedLine(_state));
                    break;
            }

            hud.SetMoney(_state.money);
        }

        private void OnRefuse()
        {
            if (_current == null || _leaving) return;
            var refused = _current.refusedChoice;
            if (ServiceResolver.Refuse(_state, refused != null ? refused.id : null, refused != null ? refused.effects : null))
                ShowChoiceResult(refused);
            Leave(_current.refusedLine);
        }

        private void Leave(string line)
        {
            customerView.Say(line);
            customerView.HideTalkOptions();
            SetActionButtons(false);
            _leaving = true;
            _leaveTimer = leaveDelaySeconds;
            _plate.Clear();
            plateView.Refresh(_plate);
        }

        private void FinishCustomer()
        {
            customerView.Hide();
            _current = null;
            _leaving = false;
        }

        private void SetActionButtons(bool on)
        {
            if (serveButton != null) serveButton.interactable = on;
            if (refuseButton != null) refuseButton.interactable = on;
        }

        private string OrderHint(OrderSpec order)
        {
            if (order == null || order.IsEmpty) return "";
            var parts = new List<string>();
            if (order.wantsDurum)
            {
                var fillings = order.fillings.Select(database.IngredientName).ToList();
                parts.Add(fillings.Count > 0 ? $"Dürüm ({string.Join(", ", fillings)})" : "Sade dürüm");
            }
            if (!string.IsNullOrEmpty(order.drink))
                parts.Add(database.IngredientName(order.drink));
            return "Sipariş: " + string.Join("  ·  ", parts);
        }

        // ---------- Preparing ----------

        private void OnIngredientPicked(IngredientData ingredient)
        {
            if (_ended || ingredient == null) return;

            BuildResult result;
            switch (ingredient.kind)
            {
                case IngredientKind.Base: result = _plate.AddBase(); break;
                case IngredientKind.Filling: result = _plate.AddFilling(ingredient.id); break;
                default: result = _plate.SetDrink(ingredient.id); break;
            }

            ReportBuild(result);
            plateView.Refresh(_plate);
        }

        private void OnWrap()
        {
            if (_ended) return;
            ReportBuild(_plate.Wrap());
            plateView.Refresh(_plate);
        }

        private void OnTrash()
        {
            if (_ended || _plate.IsEmpty) return;
            _plate.Clear();
            plateView.Refresh(_plate);
            toast.Show("Tabağı boşalttın.", 1.2f);
        }

        private void ReportBuild(BuildResult result)
        {
            switch (result)
            {
                case BuildResult.NeedsBase: toast.Show("Önce lavaşı ser.", 1.5f); break;
                case BuildResult.AlreadyHasBase: toast.Show("Tezgâhta zaten bir lavaş var.", 1.5f); break;
                case BuildResult.AlreadyWrapped: toast.Show("Dürüm sarıldı, artık bir şey eklenmez.", 1.5f); break;
                case BuildResult.Duplicate: toast.Show("Bu zaten var.", 1.2f); break;
                case BuildResult.AlreadyHasDrink: toast.Show("Tabakta zaten içecek var.", 1.5f); break;
            }
        }

        // ---------- Rule paper ----------

        private void OpenRulePaper()
        {
            if (_ended) return;
            rulePaper.Open(database.RulesUpTo(_night.nightNumber), database.BackRuleFor(_night.nightNumber),
                _state.curiosity, _state.playerName);
        }

        private void OnPaperSigned(string playerName)
        {
            if (_state.HasSigned) return;
            _state.playerName = playerName;
            ApplyChoice(_night.signNameChoice);
        }

        private void OnPaperFlipped() => ApplyChoice(_night.flipPaperChoice);

        // ---------- Radio, anomalies, choices ----------

        private void OnWhisperHeard()
        {
            whisper.Show(_night.whisperText, 2.5f);
            if (tension != null) tension.Spike(0.35f);
            ApplyChoice(_night.radioWhisperChoice);
        }

        private void UpdateAnomalies()
        {
            for (int i = 0; i < _anomalyFired.Length; i++)
            {
                if (_anomalyFired[i] || _clock.Progress < _anomalyProgress[i]) continue;
                _anomalyFired[i] = true;
                var anomaly = _night.anomalies[i];
                switch (anomaly.type)
                {
                    case AnomalyType.DonerFlip:
                        if (doner != null) doner.Flip(anomaly.durationSeconds);
                        if (tension != null) tension.Spike(0.15f);
                        break;
                    default:
                        Debug.Log($"[Counter] Anomali henüz uygulanmadı: {anomaly.type}");
                        break;
                }
            }
        }

        private void ApplyChoice(ChoiceData choice)
        {
            if (choice == null) return;
            if (CounterSystem.TryApplyChoice(_state, choice.id, choice.effects))
                ShowChoiceResult(choice);
        }

        private void ShowChoiceResult(ChoiceData choice)
        {
            if (choice == null) return;
            if (!string.IsNullOrEmpty(choice.resultText)) toast.Show(choice.resultText, 3.5f);
            if (tension == null) return;
            if (choice.flickerLights) tension.Flicker(1.2f);
            if (choice.tensionSpike > 0f) tension.Spike(choice.tensionSpike);
        }

        // ---------- End ----------

        private IEnumerator EndNight()
        {
            _ended = true;
            SetActionButtons(false);
            rulePaper.Close();
            toast.Show(_night.endText, endDelaySeconds);

            GameSession.LastNightNumber = _night.nightNumber;
            GameSession.LastNightEarnings = _state.money - _moneyAtStart;
            _state.AdvanceNight();
            GameSession.Save();

            yield return new WaitForSeconds(endDelaySeconds);
            GameSession.Load(SceneNames.NightEnd);
        }
    }
}
