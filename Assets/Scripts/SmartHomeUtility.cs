using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class SmartHomeUtility : MonoBehaviour
{
    static System.Random randomNumber = new System.Random();

    public Camera house_camera;
    public Camera kitchen_camera;
    public Camera bathroom_camera;
    public Camera room_camera;
    // Used for the set the "seconds" to send API request to the server
    [SerializeField] private int timer = 5;
    public bool leakage { get; set; }
    // Used for the set the "secods" to measure the usage of utilizations
    [SerializeField] private int updateTimer = 5; // Seconds
    //ELECTRIITY
    public decimal totalElecUsage = 0.0000m;
    public decimal elecDefaultValue = 0.0012m;
    public decimal currentElecUsage = 0.0m;

    // WATER
    public decimal totalWaterUsage = 0.000m;
    public bool checkWaterLeakage = false;
    // Due to the fact that the usage of the water in seconds was so small, we decided to change Litres to mL
    public decimal waterDefaultUsage = 0.129m;
    public decimal currentWaterUsage = 0.0m;

    //GAS
    public decimal totalGasUsage = 0.000m;
    double currentGasUsage = 0.0d;
    // Due to the fact that the usage of the gas in seconds was so small, we decided to change M3 to Cm3
    public float gasDeaultUsage = 0.25f; // Usage of gas/sec (cm3)
    public int pipeTemperature = 40;
    public decimal gasPressure = 0.5m;
    public int velocity = 15;
    public bool checkLeak = false;
    // Used for measuring the gas sensors and measurements
    public Light light_1;
    public Light light_2;
    public ParticleSystem sink_water;
    public ParticleSystem shower_water;
    public ParticleSystem stove_1;
    public ParticleSystem stove_2;
    public ParticleSystem gas_leakage;
    public ParticleSystem elecOutage;
    public ParticleSystem waterLeakage;

    public bool checkPreAndTem = false;
    // Start is called before the first frame update
    void Start()
    {
        // Sending Requeset to API after "timer" seconds
        StartCoroutine(SendingRequestToAPI(timer));
        StartCoroutine(measuringElecUsage(updateTimer));
        StartCoroutine(measuringWaterUsage(updateTimer));
        StartCoroutine(measuringGasUsage(updateTimer));
        StartCoroutine(checkGasLeakage(updateTimer));
        // StartCoroutine(checkWaterLeakages(updateTimer));

        house_camera.GetComponent<Camera>().enabled = true;
        kitchen_camera.GetComponent<Camera>().enabled = false;
        bathroom_camera.GetComponent<Camera>().enabled = false;
        room_camera.GetComponent<Camera>().enabled = false;

        light_1.GetComponent<Light>().enabled = false;
        light_2.GetComponent<Light>().enabled = false;
        sink_water.GetComponent<ParticleSystem>().Stop();
        shower_water.GetComponent<ParticleSystem>().Stop();
        stove_1.GetComponent<ParticleSystem>().Stop();
        stove_2.GetComponent<ParticleSystem>().Stop();
        gas_leakage.GetComponent<ParticleSystem>().Stop();
        elecOutage.GetComponent<ParticleSystem>().Stop();
        waterLeakage.GetComponent<ParticleSystem>().Stop();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("1"))
        {
            house_camera.enabled = true;
            kitchen_camera.enabled = false;
            bathroom_camera.enabled = false;
            room_camera.enabled = false;
        }
        else if (Input.GetKeyDown("2"))
        {
            house_camera.enabled = false;
            kitchen_camera.enabled = true;
            bathroom_camera.enabled = false;
            room_camera.enabled = false;
        }
        else if (Input.GetKeyDown("3"))
        {
            house_camera.enabled = false;
            kitchen_camera.enabled = false;
            bathroom_camera.enabled = true;
            room_camera.enabled = false;
        }
        else if (Input.GetKeyDown("4"))
        {
            house_camera.enabled = false;
            kitchen_camera.enabled = false;
            bathroom_camera.enabled = false;
            room_camera.enabled = true;
        }

        if (Input.GetKeyDown("8"))
        {
            if (sink_water.isStopped)
            {
                sink_water.Play();
            }
            else if (sink_water.isPlaying)
            {
                sink_water.Stop();
            }
        }
        if (Input.GetKeyDown("9"))
        {
            if (shower_water.isStopped)
            {
                shower_water.Play();
            }
            else if (shower_water.isPlaying)
            {
                shower_water.Stop();
            }
        }
        if (Input.GetKeyDown("g"))
        {
            if (gas_leakage.isStopped)
            {
                checkPreAndTem = true;
                gas_leakage.Play();
            }

            else if (gas_leakage.isPlaying)
            {
                gas_leakage.Stop();
            }
        }
        if (Input.GetKeyDown("f"))
        {
            if (stove_1.isStopped && stove_2.isStopped)
            {
                checkPreAndTem = true;
                stove_1.Play();
                stove_2.Play();
            }
            else if (stove_1.isPlaying || stove_2.isPlaying)
            {
                stove_1.Stop();
                stove_2.Stop();
            }
        }
        if (Input.GetKeyDown("p"))
        {
            pipeTemperature = 50;
            gasPressure = 2m;
        }
        if (Input.GetKeyDown("["))
        {
            pipeTemperature = 40;
            gasPressure = 0.5m;
        }
        if (Input.GetKeyDown("e"))
        {
            if (elecOutage.isStopped)
            {
                light_1.enabled = false;
                light_2.enabled = false;
                elecOutage.Play();
                StopAllCoroutines();
            }
            else if (elecOutage.isPlaying)
            {
                elecOutage.Stop();
            }
        }
        if (Input.GetKeyDown("w"))
        {
            if (waterLeakage.isStopped)
            {
                checkWaterLeakage = true;
                waterLeakage.Play();
            }
            else if (waterLeakage.isPlaying)
            {
                checkWaterLeakage = false;
                waterLeakage.Stop();
            }
        }
    }

    IEnumerator SendingRequestToAPI(int timer)
    {
        // Send requeset using API after delaying for "timer" seconds.
        while (true)
        {
            yield return new WaitForSeconds(timer);
            SendRequest();
        }
    }

    IEnumerator measuringElecUsage(int updateTimer)
    {
        // Send requeset using API after delaying for "timer" seconds.
        while (true)
        {
            yield return new WaitForSeconds(updateTimer);
            if ((light_1.enabled && !light_2.enabled) || (!light_1.enabled && light_2.enabled))
                currentElecUsage += elecDefaultValue;
            // Light 1 = 1 && Light2 = 1
            else if (light_1.enabled && light_2.enabled)
                currentElecUsage += elecDefaultValue * 2;
            else
                currentElecUsage = 0.0m;
            totalElecUsage += currentElecUsage;

            // Debug.Log("Elec Usage : " + currentElecUsage);
        }
    }
    IEnumerator measuringWaterUsage(int updateTimer)
    {
        // Send requeset using API after delaying for "timer" seconds.
        while (true)
        {
            yield return new WaitForSeconds(updateTimer);
            // Send usage of water for each objects that are running for water
            if ((sink_water.isPlaying && !shower_water.isPlaying && !waterLeakage.isPlaying)
            || (!sink_water.isPlaying && shower_water.isPlaying && !waterLeakage.isPlaying)
            || (!sink_water.isPlaying && !shower_water.isPlaying && waterLeakage.isPlaying))
                currentWaterUsage = waterDefaultUsage;
            // Sink = 1 && Shower = 1 && Water Leakage = 0
            else if ((sink_water.isPlaying && shower_water.isPlaying && !waterLeakage.isPlaying)
            || (!sink_water.isPlaying && shower_water.isPlaying && waterLeakage.isPlaying)
            || (sink_water.isPlaying && !shower_water.isPlaying && waterLeakage.isPlaying))
                currentWaterUsage = waterDefaultUsage * 2;
            // If all of the obecjts in the system is showing the usage of water.
            else if ((sink_water.isPlaying && shower_water.isPlaying && waterLeakage.isPlaying))
                currentWaterUsage = waterDefaultUsage * 3;
            else
                currentWaterUsage = 0.0m;
            totalWaterUsage += currentWaterUsage;
            // Debug.Log("Water Usage : " + currentWaterUsage);
            // Debug.Log("Water Leakage " + checkWaterLeakage);
        }
    }
    IEnumerator measuringGasUsage(int updateTimer)
    {
        // Send requeset using API after delaying for "timer" seconds.
        while (true)
        {
            yield return new WaitForSeconds(updateTimer);
            if (stove_1.isPlaying)
            {
                double rand_num = randomNumber.Next(25, 29);
                currentGasUsage = rand_num / 100;
                Debug.Log(currentGasUsage);
                totalGasUsage += (decimal)currentGasUsage;
            }
            else
                currentGasUsage = 0.0d;
            // Debug.Log("Gas Usage : " + totalGasUsage);
            // Debug.Log("Pipe Temprature: " + pipeTemperature + ", \nGas Pressure: " + gasPressure);
        }
    }
    IEnumerator checkGasLeakage(int updateTimer)
    {
        // Send requeset using API after delaying for "timer" seconds.
        while (true)
        {
            yield return new WaitForSeconds(updateTimer);
            if (gas_leakage.isPlaying)
            {
                checkLeak = true;
                currentGasUsage = randomNumber.Next(25, 29);
                currentGasUsage = currentGasUsage / 100;
                totalGasUsage += (decimal)currentGasUsage;
                pipeTemperature = 36;
                gasPressure = 0.45m;
                velocity = 15;
            }
            else
            {
                if (checkPreAndTem)
                {
                    checkLeak = false;
                    pipeTemperature = 40;
                    gasPressure = 0.5m;
                    velocity = 15;
                }
            }
        }
    }
    public async Task SendRequest()
    {
        var req = new Request
        {
            id = "iNAanuUM", // id
            type = "centralHub", // type
            devices = new List<IDevice>{ // devices
        new GasDevice{
            id = "BuRWtPeN",
             type = "GAS_METER",
              consumption = new Consumption{value = (decimal)currentGasUsage, unit = "CM3" },
              gasDetectSensor = new GasDetectSensor{
                id = "GASMAX_CX",
                 leakage = checkLeak,
                  gasComposition = new[] {"Methane_LEL", "CO2", "Propane_LEL", "Methane_Vol"}},
              gasFlowSensor = new GasFlowSensor
        {
            id = "FLOWSIC500",
            pressure = new UnitBase("PSI", gasPressure),
            temperature = new UnitBase("C", pipeTemperature),
            velocity = new UnitBase("m/s", ((byte)velocity))
        }},
        new Device{
            id = "FKUsIR3l",
             type = "ELECTRICITY_METER",
              consumption = new Consumption
              {
                value = currentElecUsage,
                 unit = "wH",
                 }},
        new ElectricityDevice{
            id = "OfnZpXDb",
            leakage = checkWaterLeakage,
             type = "WATER_METER",
              consumption = new Consumption{
                value = currentWaterUsage,
                unit = "mL"}},
    }
        };

        var s = JsonConvert.SerializeObject(req, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        });

        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("token", "gy93ud5w");
        var response = httpClient.PostAsync("https://qj1yd7xm4a.execute-api.us-east-1.amazonaws.com/smart-utility-iot/devices", new StringContent(s)).GetAwaiter().GetResult();

        if (response.IsSuccessStatusCode)
        {
            var j = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var json = JsonConvert.DeserializeObject<dynamic>(j);
            Debug.Log(j);
            // if ((int)json.Status == 200) //200 - 299
            // {
            //     if (GameLight.enabled == true)
            //         GameLight.enabled = !GameLight.enabled;
            //     else
            //         GameLight.enabled = GameLight.enabled;
            // }
        }
    }
    public class UnitBase
    {
        public UnitBase()
        {

        }

        public UnitBase(string unit, decimal value)
        {
            this.unit = unit;
            this.value = value;
        }
        public string unit { get; set; }
        public decimal value { get; set; }
    }
    public class Pressure : UnitBase
    {
    }

    public class Consumption : UnitBase
    {
    }
    public class Gas_Meter : UnitBase
    {
        public string type = "Gas_Meter";
    }
    public class gasDetectSensor : UnitBase
    {

    }
    public interface IIdentifier
    {
        string id { get; set; }
        string type { get; set; }
    }
    public class Request : IIdentifier
    {
        public string type { get; set; }
        public string id { get; set; }
        public List<IDevice> devices { get; set; }
    }

    public interface IDevice : IIdentifier
    {
        Consumption consumption { get; set; }
    }
    public class Device : IDevice
    {
        public string type { get; set; }
        public string id { get; set; }
        public Consumption consumption { get; set; }
    }

    public class GasDevice : Device
    {
        public GasFlowSensor gasFlowSensor { get; set; }
        public GasDetectSensor gasDetectSensor { get; set; }
    }

    public class GasDetectSensor
    {
        public string id { get; set; }
        public bool leakage { get; set; }
        public string[] gasComposition { get; set; }
    }
    public class GasFlowSensor
    {
        public string id { get; set; }
        public UnitBase pressure { get; set; }
        public UnitBase temperature { get; set; }
        public UnitBase velocity { get; set; }
    }
    public class ElectricityDevice : IDevice
    {
        public string id { get; set; }
        public bool leakage { get; set; }
        public string type { get; set; }
        public Consumption consumption { get; set; }
    }
}
