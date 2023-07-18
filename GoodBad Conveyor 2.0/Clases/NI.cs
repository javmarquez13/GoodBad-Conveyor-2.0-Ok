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
                TaskReadIn.Dispose(); //Validacion
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

                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                _writer.WriteSingleSampleMultiLine(true, outputs);
                TaskWriteOut.Dispose(); //Validation
            }
            catch (DaqException ex)
            {
                Globals.DAQ_OK = false;
                LogEvents.RegisterEvent(14, "WriteDAQ: " + ex.Message);
            }

            return outputs;
        }


        /// <summary>
        /// Function to write individually the outputs on DAQ.
        /// </summary>
        /// <param name="out0">NG for Lane 1</param>
        /// <param name="out1">Turn on conveyor</param>
        /// <param name="out2">OK for Lane 1</param>
        /// <param name="out3">NG for Lane 2</param>
        /// <param name="out4">Not used </param>
        /// <param name="out5">OK for Lane 2</param>
        /// <param name="out6">Not used</param>
        /// <param name="out7">Not used</param>
        /// <returns></returns>
        public bool[] WriteDAQOutputs(bool out0, bool out1, bool out2, bool out3, bool out4, bool out5, bool out6, bool out7)
        {
            bool[] outputs = { out0, out1, out2, out3, out4, out5, out6, out7 };

            try
            {
                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                _myDOChannel = TaskWriteOut.DOChannels.CreateChannel(Globals.DAQ_NAME + @"/" + "port" + 0, "write0", ChannelLineGrouping.OneChannelForAllLines);
                _writer = new DigitalSingleChannelWriter(TaskWriteOut.Stream);

                TaskWriteOut = new NationalInstruments.DAQmx.Task();

                _writer.WriteSingleSampleMultiLine(true, outputs);
                TaskWriteOut.Dispose(); //Validation
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
