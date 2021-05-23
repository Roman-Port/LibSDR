using RomanPort.LibSDR.Components;
using RomanPort.LibSDR.Components.Digital.RDS.Client;
using RomanPort.LibSDR.Components.Digital.RDS.Data.Commands;
using RomanPort.LibSDR.Components.Filters.Builders;
using RomanPort.LibSDR.Components.Filters.FIR.ComplexFilter;
using RomanPort.LibSDR.Demodulators.Analog.Broadcast;
using RomanPort.LibSDR.Hardware.AirSpy;
using RomanPort.LibSDR.IO.USB.LibUSB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RomanPort.LibSDR.Utils.RdsSpy
{
    public unsafe partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const int BUFFER_SIZE = 65536;
        private const int BANDWIDTH = 200000;

        private UnsafeBuffer iqBuffer;
        private Complex* iqBufferPtr;
        private UnsafeBuffer audioBuffer;
        private float* audioBufferPtr;

        private LibUSBProvider usb;
        private AirSpyDevice device;
        private IComplexFirFilter filter;
        private WbFmDemodulator fm;
        private RdsClient rds;

        private Thread worker;

        private void Form1_Load(object sender, EventArgs e)
        {
            //Add all entries to the pie graph to start off with
            for(int i = 0; i<32; i++)
            {
                rdsGroupChart.Series["Groups"].Points.AddXY("", 0);
                rdsGroupChart.Series["Groups"].Points[i].IsVisibleInLegend = false;
            }

            //Configure table
            rdsHistory.Columns.Add(GenerateColumn("PI", 40));
            rdsHistory.Columns.Add(GenerateColumn("Group", 50));
            rdsHistory.Columns.Add(GenerateColumn("Description", -1));
            
            //Create buffers
            iqBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out iqBufferPtr);
            audioBuffer = UnsafeBuffer.Create(BUFFER_SIZE, out audioBufferPtr);

            //Open USB device
            usb = new LibUSBProvider();
            device = AirSpyDevice.OpenDevice(usb);

            //Configure radio
            device.SetLinearGain(gain.Value / 100f);
            device.CenterFrequency = (long)(freq.Value * 1000000);
            device.SampleRate = 3000000;
            device.StartRx();

            //Create filter
            var filterBuilder = new LowPassFilterBuilder(device.SampleRate, BANDWIDTH / 2)
                .SetAutomaticTapCount(BANDWIDTH * 0.1f, 50)
                .SetWindow();
            filter = ComplexFirFilter.CreateFirFilter(filterBuilder, filterBuilder.GetDecimation(out float decimatedSampleRate));

            //Create FM and RDS
            fm = new WbFmDemodulator();
            fm.Configure(BUFFER_SIZE, decimatedSampleRate, 48000);
            fm.OnStereoDetected += Fm_OnStereoDetected;
            fm.OnRdsDetected += Fm_OnRdsDetected;
            rds = new RdsClient();
            fm.OnRdsFrameEmitted += (ulong frame) => rds.ProcessFrame(frame);

            //Bind RDS client commands
            rds.ProgramService.OnPartialTextReceived += ProgramService_OnPartialTextReceived;
            rds.PiCode.OnPiCodeChanged += PiCode_OnPiCodeChanged;
            rds.ProgramType.OnCategoryChanged += ProgramType_OnCategoryChanged;
            rds.RadioText.OnPartialTextReceived += RadioText_OnPartialTextReceived;
            rds.OnCommand += Rds_OnCommand;

            //Create worker thread
            worker = new Thread(WorkerThread);
            worker.IsBackground = true;
            worker.Start();
        }

        private static DataGridViewColumn GenerateColumn(string text, int width)
        {
            DataGridViewColumn col = new DataGridViewColumn();
            col.Name = text;
            col.HeaderText = text;
            col.DataPropertyName = text;
            col.CellTemplate = new DataGridViewTextBoxCell();
            if (width == -1)
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            else
                col.Width = width;
            return col;
        }

        private int[] groupChartValues = new int[32]; //32 = 2^5

        private void Rds_OnCommand(RdsClient client, RdsCommand command)
        {
            //Configure pie graph
            int groupId = (command.GroupType << 1) | (command.GroupVersionB ? 1 : 0);

            //Update internal count
            groupChartValues[groupId]++;

            //Update UI
            Invoke((MethodInvoker)delegate
            {
                //Update graph
                rdsGroupChart.Series["Groups"].Points[groupId].SetValueXY(command.GroupName + " (" + groupChartValues[groupId] + ")", groupChartValues[groupId]);
                rdsGroupChart.Series["Groups"].Points[groupId].IsVisibleInLegend = true;
                rdsGroupChart.Invalidate();

                //Update table
                if(!historyPause.Checked)
                    rdsHistory.Rows.Insert(0, command.PiCode.ToString("X"), command.GroupName, command.DescribeCommand());
            });
        }

        private void Fm_OnRdsDetected(bool stereoDetected)
        {
            Invoke((MethodInvoker)delegate
            {
                indRds.BackColor = stereoDetected ? COLOR_DETECTED : COLOR_LOST;
            });
        }

        private static readonly Color COLOR_DETECTED = Color.FromArgb(17, 247, 55);
        private static readonly Color COLOR_LOST = Color.FromArgb(224, 224, 224);

        private void Fm_OnStereoDetected(bool stereoDetected)
        {
            Invoke((MethodInvoker)delegate
            {
                indStereo.BackColor = stereoDetected ? COLOR_DETECTED : COLOR_LOST;
            });
        }

        private void WorkerThread()
        {
            int read;
            while(true)
            {
                read = device.Read(iqBufferPtr, BUFFER_SIZE, 0);
                read = filter.Process(iqBufferPtr, read);
                read = fm.Demodulate(iqBufferPtr, audioBufferPtr, read);
            }
        }

        private void RadioText_OnPartialTextReceived(RdsClient ctx, char[] buffer, int offset)
        {
            Invoke((MethodInvoker)delegate
            {
                rdsRt.Text = new string(buffer);
            });
        }

        private void ProgramType_OnCategoryChanged(RdsClient ctx, byte category)
        {
            Invoke((MethodInvoker)delegate
            {
                rdsCategory.Text = category + " : " + ctx.ProgramType.CategoryAmerica.ToString();
            });
        }

        private void PiCode_OnPiCodeChanged(RdsClient ctx, ushort pi)
        {
            Invoke((MethodInvoker)delegate
            {
                rdsPi.Text = pi.ToString("X");
            });
        }

        private void ProgramService_OnPartialTextReceived(RdsClient ctx, char[] buffer, int offset)
        {
            Invoke((MethodInvoker)delegate
            {
                rdsPs.Text = new string(buffer);
            });
        }

        private void gain_Scroll(object sender, EventArgs e)
        {
            device.SetLinearGain(gain.Value / 100f);
        }

        private void freq_ValueChanged(object sender, EventArgs e)
        {
            device.CenterFrequency = (long)(freq.Value * 1000000);
            Reset();
        }

        private void rdsGraph_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void Reset()
        {
            //Reset RDS
            rds.Reset();

            //Update UI
            Invoke((MethodInvoker)delegate
            {
                //Update graph
                for(int i = 0; i<32; i++)
                {
                    groupChartValues[i] = 0;
                    rdsGroupChart.Series["Groups"].Points[i].SetValueXY("", 0);
                    rdsGroupChart.Series["Groups"].Points[i].IsVisibleInLegend = false;
                }
                rdsGroupChart.Invalidate();

                //Update table
                rdsHistory.Rows.Clear();
            });
        }
    }
}
