from fastapi import FastAPI, Response

app = FastAPI()

@app.get("/")
def cookie(response: Response):
    response.set_cookie(key="mysession", value="1242r")
    return {"Hello": "redfish world."}

@app.get("/api")
def read_root():
    return {"firstName":"Frodo","lastName":"Baggins","address":{"street":"Bag End","state":"The Shire","country":"Middle Earth"},"_links":{"self":{"href":"/api/myresource"}}}
    
@app.get("/api/myresource")
def read_root():
    return {"fruits":["Apple","Orange","Banana","Melon"]}
    
@app.get("/redfish/v1")
def read_root():
    return {"@odata.type":"#ServiceRoot.v1_14_0.ServiceRoot","Id":"RootService","Name":"Root Service","RedfishVersion":"1.15.0","UUID":"92384634-2938-2342-8820-489239905423","ProtocolFeaturesSupported":{"ExpandQuery":{"ExpandAll":True,"Levels":True,"MaxLevels":6,"Links":True,"NoLinks":True},"SelectQuery":False,"FilterQuery":False,"OnlyMemberQuery":True,"ExcerptQuery":True},"Systems":{"@odata.id":"/redfish/v1/Systems"},"Chassis":{"@odata.id":"/redfish/v1/Chassis"},"Managers":{"@odata.id":"/redfish/v1/Managers"},"Tasks":{"@odata.id":"/redfish/v1/TaskService"},"SessionService":{"@odata.id":"/redfish/v1/SessionService"},"AccountService":{"@odata.id":"/redfish/v1/AccountService"},"EventService":{"@odata.id":"/redfish/v1/EventService"},"Registries":{"@odata.id":"/redfish/v1/Registries"},"UpdateService":{"@odata.id":"/redfish/v1/UpdateService"},"CertificateService":{"@odata.id":"/redfish/v1/CertificateService"},"KeyService":{"@odata.id":"/redfish/v1/KeyService"},"Links":{"Sessions":{"@odata.id":"/redfish/v1/SessionService/Sessions"}},"ComponentIntegrity":{"@odata.id":"/redfish/v1/ComponentIntegrity"},"Oem":{},"@odata.id":"/redfish/v1/","@odata.etag":"xxxxxxxxxx"}

@app.get("/redfish/v1/Systems")
def read_root():
    return {"@odata.type":"#ComputerSystemCollection.ComputerSystemCollection","Name":"Computer System Collection","Members@odata.count":1,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2"}],"@odata.id":"/redfish/v1/Systems"}

@app.get("/redfish/v1/Systems/437XR1138R2")
def read_root():
    return {"@odata.type":"#ComputerSystem.v1_19_0.ComputerSystem","Id":"437XR1138R2","Name":"WebFrontEnd483","SystemType":"Physical","AssetTag":"Chicago-45Z-2381","Manufacturer":"Contoso","Model":"3500","SubModel":"RX","SKU":"8675309","SerialNumber":"437XR1138R2","PartNumber":"224071-J23","Description":"Web Front End node","UUID":"38947555-7742-3448-3784-823347823834","HostName":"web483","Status":{"State":"Enabled","Health":"OK","HealthRollup":"OK"},"HostingRoles":["ApplicationServer"],"IndicatorLED":"Off","PowerState":"On","Boot":{"BootSourceOverrideEnabled":"Once","BootSourceOverrideTarget":"Pxe","BootSourceOverrideTarget@Redfish.AllowableValues":["None","Pxe","Cd","Usb","Hdd","BiosSetup","Utilities","Diags","SDCard","UefiTarget"],"BootSourceOverrideMode":"UEFI","UefiTargetBootSourceOverride":"/0x31/0x33/0x01/0x01"},"TrustedModules":[{"FirmwareVersion":"1.13b","InterfaceType":"TPM1_2","Status":{"State":"Enabled","Health":"OK"}}],"Oem":{"Contoso":{"@odata.type":"#Contoso.ComputerSystem","ProductionLocation":{"FacilityName":"PacWest Production Facility","Country":"USA"}},"Chipwise":{"@odata.type":"#Chipwise.ComputerSystem","Style":"Executive"}},"BootProgress":{"LastState":"OSRunning","LastStateTime":"2021-03-13T04:14:13+06:00","LastBootTimeSeconds":676},"LastResetTime":"2021-03-13T04:02:57+06:00","BiosVersion":"P79 v1.45 (12/06/2017)","ProcessorSummary":{"Count":2,"Model":"Multi-Core Intel(R) Xeon(R) processor 7xxx Series","LogicalProcessorCount":16,"CoreCount":8,"Status":{"State":"Enabled","Health":"OK","HealthRollup":"OK"}},"MemorySummary":{"TotalSystemMemoryGiB":96,"TotalSystemPersistentMemoryGiB":0,"MemoryMirroring":"None","Status":{"State":"Enabled","Health":"OK","HealthRollup":"OK"}},"Bios":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Bios"},"SecureBoot":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot"},"Processors":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors"},"Memory":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory"},"EthernetInterfaces":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces"},"SimpleStorage":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SimpleStorage"},"LogServices":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/LogServices"},"GraphicsControllers":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/GraphicsControllers"},"USBControllers":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/USBControllers"},"Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Certificates"},"VirtualMedia":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/VirtualMedia"},"Links":{"Chassis":[{"@odata.id":"/redfish/v1/Chassis/1U"}],"ManagedBy":[{"@odata.id":"/redfish/v1/Managers/BMC"}]},"Actions":{"#ComputerSystem.Reset":{"target":"/redfish/v1/Systems/437XR1138R2/Actions/ComputerSystem.Reset","ResetType@Redfish.AllowableValues":["On","ForceOff","GracefulShutdown","GracefulRestart","ForceRestart","Nmi","ForceOn","PushPowerButton"]},"Oem":{"#Contoso.Reset":{"target":"/redfish/v1/Systems/437XR1138R2/Oem/Contoso/Actions/Contoso.Reset"}}},"@odata.id":"/redfish/v1/Systems/437XR1138R2"}

@app.get("/redfish/v1/Systems/437XR1138R2/Bios")
def read_root():
    return {"@odata.type":"#Bios.v1_2_0.Bios","Id":"Bios","Name":"BIOS Configuration Current Settings","AttributeRegistry":"BiosAttributeRegistryP89.v1_0_0","Attributes":{"AdminPhone":"","BootMode":"Uefi","EmbeddedSata":"Raid","NicBoot1":"NetworkBoot","NicBoot2":"Disabled","PowerProfile":"MaxPerf","ProcCoreDisable":0,"ProcHyperthreading":"Enabled","ProcTurboMode":"Enabled","UsbControl":"UsbEnabled"},"ResetBiosToDefaultsPending":True,"@Redfish.Settings":{"@odata.type":"#Settings.v1_3_5.Settings","ETag":"9234ac83b9700123cc32","Messages":[{"MessageId":"Base.1.0.SettingsFailed","RelatedProperties":["#/Attributes/ProcTurboMode"]}],"SettingsObject":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Bios/Settings"},"Time":"2016-03-07T14:44.30-05:00"},"Actions":{"#Bios.ResetBios":{"target":"/redfish/v1/Systems/437XR1138R2/Bios/Actions/Bios.ResetBios"},"#Bios.ChangePassword":{"target":"/redfish/v1/Systems/437XR1138R2/Bios/Actions/Bios.ChangePassword"}},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Bios"}

@app.get("/redfish/v1/Systems/437XR1138R2/Bios/Settings")
def read_root():
    return {"@odata.type":"#Bios.v1_2_0.Bios","Id":"Settings","Name":"BIOS Configuration Pending Settings","AttributeRegistry":"BiosAttributeRegistryP89.v1_0_0","Attributes":{"AdminPhone":"(404) 555-1212","BootMode":"Uefi","EmbeddedSata":"Ahci","NicBoot1":"NetworkBoot","NicBoot2":"NetworkBoot","PowerProfile":"MaxPerf","ProcCoreDisable":0,"ProcHyperthreading":"Enabled","ProcTurboMode":"Disabled","UsbControl":"UsbEnabled"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Bios/Settings"}

@app.get("/redfish/v1/Systems/437XR1138R2/EthernetInterfaces")
def read_root():
    return {"@odata.type":"#EthernetInterfaceCollection.EthernetInterfaceCollection","Name":"Ethernet Interface Collection","Description":"System NICs on Contoso Servers","Members@odata.count":4,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B0411"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B8890"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/VLAN1"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/ToManager"}],"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces"}

@app.get("/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B0411")
def read_root():
    return {"@odata.type":"#EthernetInterface.v1_9_0.EthernetInterface","Id":"12446A3B0411","Name":"Ethernet Interface","Description":"System NIC 1","Status":{"State":"Enabled","Health":"OK"},"EthernetInterfaceType":"Physical","LinkStatus":"LinkUp","PermanentMACAddress":"12:44:6A:3B:04:11","MACAddress":"12:44:6A:3B:04:11","SpeedMbps":1000,"FullDuplex":True,"HostName":"web483","FQDN":"web483.contoso.com","IPv6DefaultGateway":"fe80::3ed9:2bff:fe34:600","NameServers":["names.contoso.com"],"IPv4Addresses":[{"Address":"192.168.0.10","SubnetMask":"255.255.252.0","AddressOrigin":"Static","Gateway":"192.168.0.1"}],"IPv6Addresses":[{"Address":"fe80::1ec1:deff:fe6f:1e24","PrefixLength":64,"AddressOrigin":"Static","AddressState":"Preferred"}],"VLAN":{"VLANEnable":False,"VLANId":101},"TeamMode":"None","@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B0411"}

@app.get("/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B8890")
def read_root():
    return {"@odata.type":"#EthernetInterface.v1_9_0.EthernetInterface","Id":"12446A3B8890","Name":"Ethernet Interface","Description":"System NIC 2","Status":{"State":"Enabled","Health":"OK"},"EthernetInterfaceType":"Physical","LinkStatus":"LinkUp","PermanentMACAddress":"12:44:6A:3B:88:90","MACAddress":"AA:BB:CC:DD:EE:00","SpeedMbps":1000,"FullDuplex":True,"HostName":"backup-web483","FQDN":"backup-web483.contoso.com","IPv6DefaultGateway":"fe80::3ed9:2bff:fe34:600","NameServers":["names.contoso.com"],"IPv4Addresses":[{"Address":"192.168.0.11","SubnetMask":"255.255.255.0","AddressOrigin":"Static","Gateway":"192.168.0.1"}],"IPv6Addresses":[{"Address":"fe80::1ec1:deff:fe6f:1e33","PrefixLength":64,"AddressOrigin":"Static","AddressState":"Preferred"}],"VLAN":{"VLANEnable":True,"VLANId":101},"TeamMode":"None","@odata.id":"/redfish/v1/Systems/437XR1138R2/EthernetInterfaces/12446A3B8890"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory")
def read_root():
    return {"@odata.type":"#MemoryCollection.MemoryCollection","Name":"Memory Module Collection","Members@odata.count":4,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM1"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM2"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM3"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM4"}],"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM1")
def read_root():
    return {"@odata.type":"#Memory.v1_16_0.Memory","Id":"DIMM1","Name":"DIMM Slot 1","RankCount":2,"MaxTDPMilliWatts":[12000],"CapacityMiB":32768,"DataWidthBits":64,"BusWidthBits":72,"ErrorCorrection":"MultiBitECC","MemoryLocation":{"Socket":1,"MemoryController":1,"Channel":1,"Slot":1},"MemoryType":"DRAM","MemoryDeviceType":"DDR4","BaseModuleType":"RDIMM","MemoryMedia":["DRAM"],"Status":{"State":"Enabled","Health":"OK"},"EnvironmentMetrics":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM1/EnvironmentMetrics"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM1"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM1/EnvironmentMetrics")
def read_root():
    return {"@odata.type":"#EnvironmentMetrics.v1_3_0.EnvironmentMetrics","Id":"EnvironmentMetrics","Name":"Memory Environment Metrics","TemperatureCelsius":{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/DIMM1Temp","Reading":44},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM1/EnvironmentMetrics"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM2")
def read_root():
    return {"@odata.type":"#Memory.v1_16_0.Memory","Id":"DIMM2","Name":"DIMM Slot 2","RankCount":2,"MaxTDPMilliWatts":[12000],"CapacityMiB":32768,"DataWidthBits":64,"BusWidthBits":72,"ErrorCorrection":"MultiBitECC","MemoryLocation":{"Socket":1,"MemoryController":1,"Channel":1,"Slot":2},"MemoryType":"DRAM","MemoryDeviceType":"DDR4","BaseModuleType":"RDIMM","MemoryMedia":["DRAM"],"Status":{"State":"Enabled","Health":"OK"},"EnvironmentMetrics":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM2/EnvironmentMetrics"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM2"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM2/EnvironmentMetrics")
def read_root():
    return {"@odata.type":"#EnvironmentMetrics.v1_3_0.EnvironmentMetrics","Id":"EnvironmentMetrics","Name":"Memory Environment Metrics","TemperatureCelsius":{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/DIMM1Temp","Reading":44},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM2/EnvironmentMetrics"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM3")
def read_root():
    return {"@odata.type":"#Memory.v1_16_0.Memory","Id":"DIMM3","Name":"DIMM Slot 3","RankCount":2,"MaxTDPMilliWatts":[12000],"CapacityMiB":32768,"DataWidthBits":64,"BusWidthBits":72,"ErrorCorrection":"MultiBitECC","MemoryLocation":{"Socket":2,"MemoryController":2,"Channel":1,"Slot":3},"MemoryType":"DRAM","MemoryDeviceType":"DDR4","BaseModuleType":"RDIMM","MemoryMedia":["DRAM"],"Status":{"State":"Enabled","Health":"OK"},"EnvironmentMetrics":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM3/EnvironmentMetrics"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM3"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM3/EnvironmentMetrics")
def read_root():
    return {"@odata.type":"#EnvironmentMetrics.v1_3_0.EnvironmentMetrics","Id":"EnvironmentMetrics","Name":"Memory Environment Metrics","TemperatureCelsius":{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/DIMM1Temp","Reading":44},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM3/EnvironmentMetrics"}

@app.get("/redfish/v1/Systems/437XR1138R2/Memory/DIMM4")
def read_root():
    return {"@odata.type":"#Memory.v1_16_0.Memory","Id":"DIMM4","Name":"DIMM Slot 4","MemoryLocation":{"Socket":2,"MemoryController":2,"Channel":1,"Slot":4},"Status":{"State":"Absent"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Memory/DIMM4"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors")
def read_root():
    return {"@odata.type":"#ProcessorCollection.ProcessorCollection","Name":"Processors Collection","Members@odata.count":3,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU1"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU2"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1"}],"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/CPU1")
def read_root():
    return {"@odata.type":"#Processor.v1_16_0.Processor","Id":"CPU1","Name":"Processor","Socket":"CPU 1","ProcessorType":"CPU","ProcessorArchitecture":"x86","InstructionSet":"x86-64","Manufacturer":"Intel(R) Corporation","Model":"Multi-Core Intel(R) Xeon(R) processor 7xxx Series","ProcessorId":{"VendorId":"GenuineIntel","IdentificationRegisters":"0x34AC34DC8901274A","EffectiveFamily":"0x42","EffectiveModel":"0x61","Step":"0x1","MicrocodeInfo":"0x429943"},"AdditionalFirmwareVersions":{"Microcode":"0x46"},"MaxSpeedMHz":3700,"OperatingSpeedMHz":2333,"OperatingSpeedRangeMHz":{"DataSourceUri":"/redfish/v1/Chassis/1U/Controls/CPU1Freq","ControlMode":"Automatic","AllowableMin":1200,"SettingMin":2000,"SettingMax":2400,"AllowableMax":3700},"TotalCores":8,"TotalThreads":16,"Status":{"State":"Enabled","Health":"OK"},"EnvironmentMetrics":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU1/EnvironmentMetrics"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU1"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/CPU1/EnvironmentMetrics")
def read_root():
    return {"@odata.type":"#EnvironmentMetrics.v1_3_0.EnvironmentMetrics","Id":"EnvironmentMetrics","Name":"Processor Environment Metrics","TemperatureCelsius":{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/CPU1Temp","Reading":44},"PowerWatts":{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/CPU1Power","Reading":12.87},"FanSpeedsPercent":[{"DataSourceUri":"/redfish/v1/Chassis/1U/Sensors/CPUFan1","DeviceName":"CPU #1 Fan Speed","Reading":80}],"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU1/EnvironmentMetrics"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/CPU2")
def read_root():
    return {"@odata.type":"#Processor.v1_16_0.Processor","Id":"CPU2","Name":"Processor","Socket":"CPU 2","ProcessorType":"CPU","Status":{"State":"Absent"},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU2"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/FPGA1")
def read_root():
    return {"@odata.type":"#Processor.v1_16_0.Processor","Id":"FPGA1","Name":"FPGA","ProcessorType":"FPGA","ProcessorArchitecture":"OEM","InstructionSet":"OEM","Manufacturer":"Intel(R) Corporation","Model":"Stratix 10","UUID":"00000000-0000-0000-0000-000000000000","Status":{"State":"Enabled","Health":"OK"},"Metrics":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/ProcessorMetrics"},"TDPWatts":120,"MaxTDPWatts":150,"ProcessorMemory":[{"IntegratedMemory":True,"MemoryType":"HBM2","CapacityMiB":512,"SpeedMHz":1066}],"FPGA":{"FpgaType":"Discrete","Model":"Stratix10","FirmwareId":"0x6400002fc614bb9","FirmwareManufacturer":"Intel(R) Corporation","FirmwareVersion":"Blue v.1.00.86","HostInterface":{"InterfaceType":"PCIe","PCIe":{"MaxPCIeType":"Gen4","MaxLanes":8}},"ExternalInterfaces":[{"InterfaceType":"Ethernet","Ethernet":{"MaxSpeedMbps":10240,"MaxLanes":4}}],"PCIeVirtualFunctions":1,"ProgrammableFromHost":True,"ReconfigurationSlots":[{"SlotId":"AFU0","UUID":"00000000-0000-0000-0000-000000000000","ProgrammableFromHost":True,"AccelerationFunction":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions/Compression"}}],"Oem":{}},"AccelerationFunctions":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions"},"Links":{"Endpoints":[],"ConnectedProcessors":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/CPU1"}]},"Actions":{"#Processor.Reset":{"target":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/Actions/Processor.Reset"}},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions")
def read_root():
    return {"@odata.type":"#AccelerationFunctionCollection.AccelerationFunctionCollection","Name":"Acceleration Function Collection","Members@odata.count":1,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions/Compression"}],"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions/Compression")
def read_root():
    return {"@odata.type":"#AccelerationFunction.v1_0_3.AccelerationFunction","Id":"Compression","Name":"Compression Accelerator","Description":"Compression Acceleration Function","Status":{"State":"Enabled","Health":"OK","HealthRollup":"OK"},"UUID":"00000000-0000-0000-0000-000000000000","FpgaReconfigurationSlots":["AFU0"],"AccelerationFunctionType":"Compression","Manufacturer":"Intel (R) Corporation","Version":"Green Compression Type 1 v.1.00.86","PowerWatts":15,"Links":{"Endpoints":[],"PCIeFunctions":[]},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/AccelerationFunctions/Compression"}

@app.get("/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/ProcessorMetrics")
def read_root():
    return {"@odata.type":"#ProcessorMetrics.v1_6_0.ProcessorMetrics","Id":"Metrics","Description":"Processor Metrics","Name":"Processor Metrics","BandwidthPercent":62,"AverageFrequencyMHz":2400,"ThrottlingCelsius":65,"TemperatureCelsius":41,"ConsumedPowerWatt":82,"FrequencyRatio":0.00432,"Cache":[{"Level":"3","CacheMiss":0.12,"HitRatio":0.719,"CacheMissesPerInstruction":0.00088,"OccupancyBytes":3030144,"OccupancyPercent":90.1}],"LocalMemoryBandwidthBytes":18253611008,"RemoteMemoryBandwidthBytes":81788928,"KernelPercent":2.3,"UserPercent":34.7,"CoreMetrics":[{"CoreId":"core0","InstructionsPerCycle":1.16,"UnhaltedCycles":6254383746,"MemoryStallCount":58372,"IOStallCount":2634872,"CoreCache":[{"Level":"2","CacheMiss":0.472,"HitRatio":0.57,"CacheMissesPerInstruction":0.00346,"OccupancyBytes":198231,"OccupancyPercent":77.4}],"CStateResidency":[{"Level":"C0","Residency":1.13},{"Level":"C1","Residency":26},{"Level":"C3","Residency":0.00878},{"Level":"C6","Residency":0.361},{"Level":"C7","Residency":72.5}]}],"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/Processors/FPGA1/ProcessorMetrics"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot")
def read_root():
    return {"@odata.type":"#SecureBoot.v1_1_0.SecureBoot","Id":"SecureBoot","Name":"UEFI Secure Boot","Actions":{"#SecureBoot.ResetKeys":{"target":"/redfish/v1/Systems/437XR1138R2/SecureBoot/Actions/SecureBoot.ResetKeys","ResetKeysType@Redfish.AllowableValues":["ResetAllKeysToDefault","DeleteAllKeys","DeletePK"]},"Oem":{}},"SecureBootEnable":False,"SecureBootCurrentBoot":"Disabled","SecureBootMode":"UserMode","SecureBootDatabases":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases"},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases")
def read_root():
    return {"@odata.type":"#SecureBootDatabaseCollection.SecureBootDatabaseCollection","Name":"UEFI SecureBoot Database Collection","Members@odata.count":8,"Members":[{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PK"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEK"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PKDefault"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEKDefault"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbDefault"},{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbxDefault"}],"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"db","Name":"db - Authorized Signature Database","Description":"UEFI db Secure Boot Database","DatabaseId":"db","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db/Certificates/"},"Signatures":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db/Signatures/"},"Actions":{"#SecureBootDatabase.ResetKeys":{"target":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db/Actions/SecureBootDatabase.ResetKeys","ResetKeysType@Redfish.AllowableValues":["ResetAllKeysToDefault","DeleteAllKeys"]},"Oem":{}},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/db"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbDefault")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"dbDefault","Name":"dbDefault - Default Authorized Signature Database","Description":"UEFI dbDefault Secure Boot Database","DatabaseId":"dbDefault","Signatures":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbDefault/Signatures/"},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbDefault"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"dbx","Name":"dbx - Forbidden Signature Database","Description":"UEFI dbx Secure Boot Database","DatabaseId":"dbx","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx/Certificates/"},"Signatures":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx/Signatures/"},"Actions":{"#SecureBootDatabase.ResetKeys":{"target":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx/Actions/SecureBootDatabase.ResetKeys","ResetKeysType@Redfish.AllowableValues":["ResetAllKeysToDefault","DeleteAllKeys"]},"Oem":{}},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbx"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbxDefault")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"dbxDefault","Name":"dbxDefault - Default Forbidden Signature Database","Description":"UEFI dbxDefault Secure Boot Database","DatabaseId":"dbxDefault","Signatures":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbxDefault/Signatures/"},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/dbxDefault"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEK")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"KEK","Name":"KEK - Key Exchange Key Database","Description":"UEFI KEK Secure Boot Database","DatabaseId":"KEK","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEK/Certificates/"},"Actions":{"#SecureBootDatabase.ResetKeys":{"target":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEK/Actions/SecureBootDatabase.ResetKeys","ResetKeysType@Redfish.AllowableValues":["ResetAllKeysToDefault","DeleteAllKeys"]},"Oem":{}},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEK"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEKDefault")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"KEKDefault","Name":"KEKDefault - Default Key Exchange Key Database","Description":"UEFI KEKDefault Secure Boot Database","DatabaseId":"KEKDefault","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEKDefault/Certificates/"},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/KEKDefault"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PK")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"PK","Name":"PK - Platform Key","Description":"UEFI PK Secure Boot Database","DatabaseId":"PK","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PK/Certificates/"},"Actions":{"#SecureBootDatabase.ResetKeys":{"target":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PK/Actions/SecureBootDatabase.ResetKeys","ResetKeysType@Redfish.AllowableValues":["ResetAllKeysToDefault","DeleteAllKeys"]},"Oem":{}},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PK"}

@app.get("/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PKDefault")
def read_root():
    return {"@odata.type":"#SecureBootDatabase.v1_0_1.SecureBootDatabase","Id":"PKDefault","Name":"PKDefault - Default Platform Key","Description":"UEFI PKDefault Secure Boot Database","DatabaseId":"PKDefault","Certificates":{"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PKDefault/Certificates/"},"Oem":{},"@odata.id":"/redfish/v1/Systems/437XR1138R2/SecureBoot/SecureBootDatabases/PKDefault"}


@app.get("/items/{item_id}")
def read_item(item_id: int, q: str = None):
    return {"item_id": item_id, "q": q}
