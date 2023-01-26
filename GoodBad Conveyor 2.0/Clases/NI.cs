using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationalInstruments.DAQmx;

namespace GoodBad_Conveyor_2._0
{
    class NI
    {
        NationalInstruments.DAQmx.Task TaskReadIn;
        NationalInstruments.DAQmx.Task TaskWriteOut;
        DIChannel myDIChannel;
        DOChannel _myDOChannel;
        DigitalSingleChannelReader _reader;
        DigitalSingleChannelWriter _writer;
        
        public bool[] ReadDAQ()
        {
            bool[] outputs = new bool[8];
            try
            {
                string[] Devices = DaqSystem.Local.Devices;

                //DaqSystem.Local.DaqWarning += Local_DaqWarning;
             
                TaskReadIn = new NationalInstruments.DAQmx.Task();
                //TaskWriteOut = new NationalInstruments.DAQmx.Task();

                myDIChannel = TaskReadIn.DIChannels.CreateChannel(Globals.DAQ_NAME + @"/" + "port" + 1, "read0", ChannelLineGrouping.OneChannelForAllLines);
                _reader = new DigitalSingleChannelReader(TaskReadIn.Stream);

                //_myDOChannel = TaskWriteOut.DOChannels.CreateChannel("DIO" + @"/" + "port" + 0, "write0", ChannelLineGrouping.OneChannelForAllLines);
                //_writer = new DigitalSingleChannelWriter(TaskWriteOut.Stream);

                //NationalInstruments.DAQmx.Task _taskNI = new NationalInstruments.DAQmx.Task();
                //TaskWriteOut = new NationalInstruments.DAQmx.Task();

                //_writer.WriteSingleSampleMultiLine(true, outputs);
                outputs = _reader.ReadSingleSampleMultiLine();
            }
            catch (DaqException ex)
            {
                Globals.DAQ_OK = false;
                LogEvents.RegisterEvent(14, "ReadDAQ: " + ex.Message);
            }

            return outputs;
        }

        private void Local_DaqWarning(object sender, DaqWarningEventArgs e)
        {
            
        }

        public bool[] WriteDAQ(bool[] outputs) 
        {
            try
            {
                //TaskReadIn = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                //myDIChannel = TaskReadIn.DIChannels.CreateChannel("DIO" + @"/" + "port" + 1, "read0", ChannelLineGrouping.OneChannelForAllLines);
                //_reader = new DigitalSingleChannelReader(TaskReadIn.Stream);

                _myDOChannel = TaskWriteOut.DOChannels.CreateChannel(Globals.DAQ_NAME + @"/" + "port" + 0, "write0", ChannelLineGrouping.OneChannelForAllLines);
                _writer = new DigitalSingleChannelWriter(TaskWriteOut.Stream);

                //NationalInstruments.DAQmx.Task _taskNI = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                _writer.WriteSingleSampleMultiLine(true, outputs);
                //outputs = _reader.ReadSingleSampleMultiLine();
            }
            catch (DaqException ex)
            {
                Globals.DAQ_OK = false;
                LogEvents.RegisterEvent(14, "WriteDAQ: " + ex.Message);
            }

            return outputs;
        }


        // FUNCTION TO PREPARE GOOD BAD CONVEYOR TO WORK BOTH LANES AT THE SAME TIME.
        public bool[] WriteDAQSingleLine(bool[] outputs)
        {
            try
            {
                //TaskReadIn = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                //myDIChannel = TaskReadIn.DIChannels.CreateChannel("DIO" + @"/" + "port" + 1, "read0", ChannelLineGrouping.OneChannelForAllLines);
                //_reader = new DigitalSingleChannelReader(TaskReadIn.Stream);

                _myDOChannel = TaskWriteOut.DOChannels.CreateChannel(Globals.DAQ_NAME + @"/" + "port" + 0, "write0", ChannelLineGrouping.OneChannelForEachLine);
                _writer = new DigitalSingleChannelWriter(TaskWriteOut.Stream);

                //NationalInstruments.DAQmx.Task _taskNI = new NationalInstruments.DAQmx.Task();
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                _writer.WriteSingleSampleSingleLine(true, true);
                //outputs = _reader.ReadSingleSampleMultiLine();
            }
            catch (DaqException ex)
            {
                Globals.DAQ_OK = false;
                LogEvents.RegisterEvent(14, "WriteDAQ: " + ex.Message);
            }

            return outputs;
        }
    }
}
