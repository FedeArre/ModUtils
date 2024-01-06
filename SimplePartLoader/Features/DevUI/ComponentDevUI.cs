using SimplePartLoader.CarGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace SimplePartLoader.Features
{
    internal class ComponentDevUI : MonoBehaviour
    {
        Text m_frameComponent;
        Text m_consoleComponent;

        Dropdown m_carDropdown;
        Dropdown m_modsDropdown;

        Toggle m_modReportOnlyWrongToggle;

        FirstPersonAIO FPS_AIO;

        void Start()
        {
            FPS_AIO = ModUtils.GetPlayer().GetComponent<FirstPersonAIO>();
            
            m_frameComponent = transform.Find("Panel/Frames").GetComponent<Text>();
            m_consoleComponent = transform.Find("Panel/Console").GetComponent<Text>();
            m_carDropdown = transform.Find("Panel/Emulator/Dropdown").GetComponent<Dropdown>();
            m_modsDropdown = transform.Find("Panel/Reports/Dropdown").GetComponent<Dropdown>();
            m_modReportOnlyWrongToggle = transform.Find("Panel/Reports/Toggle").GetComponent<Toggle>();

            m_carDropdown.options.Clear();
            m_modsDropdown.options.Clear();

            transform.Find("Panel/Reports/Button").GetComponent<Button>().onClick.AddListener(GenerateModReport);
            transform.Find("Panel/Emulator/Button").GetComponent<Button>().onClick.AddListener(JunkyardEmulator);

            // Load cars into list
            GameObject[] Cars = GameObject.Find("CarsParent").GetComponent<CarList>().Cars;

            foreach (GameObject go in Cars)
            {
                Dropdown.OptionData newOption = new Dropdown.OptionData();
                newOption.text = go.name;
                
                m_carDropdown.options.Add(newOption);
            }

            // Load mods into list
            foreach(ModInstance m in ModUtils.RegisteredMods)
            {
                Dropdown.OptionData newOption = new Dropdown.OptionData();
                newOption.text = m.Mod.ID;

                m_modsDropdown.options.Add(newOption);
            }

        }

        void Update()
        {
            int frames = (int)(Time.frameCount / Time.time);
            m_frameComponent.text = $"{frames}";

            if(Input.GetKeyDown(KeyCode.DownArrow))
            {
                if(FPS_AIO.controllerPauseState)
                    FPS_AIO.ControllerUnPause();
                else
                    FPS_AIO.ControllerPause();
            }

            m_consoleComponent.text = GetConsoleText();
        }

        public void GenerateModReport() 
        {
            ModInstance mi = ModUtils.RegisteredMods[m_modsDropdown.value];

            GameObject tempObj = new GameObject("tempReporter");
            ModReportStatus mrs = tempObj.AddComponent<ModReportStatus>();

            mrs.m_mod = mi;
            mrs.m_onlyWrongStuff = m_modReportOnlyWrongToggle.isOn;
        }

        public void JunkyardEmulator() 
        {
            GameObject[] Cars = GameObject.Find("CarsParent").GetComponent<CarList>().Cars;
            GameObject carToSpawn = Cars[m_carDropdown.value];

            EmulatedJunkyard.SpawnCar(carToSpawn);
        }

        public string GetConsoleText()
        {
            string lines = "";

            foreach(string line in CustomLogger.GetLines())
            {
                lines += line + "\n";
            }

            return lines;
        }
    }
}
