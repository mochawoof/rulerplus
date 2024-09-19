using System.Diagnostics;
using System.Runtime.InteropServices;

namespace rulerplus
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        [DllImport("user32.dll")]
        public static extern uint GetDpiForSystem();

        private string VERSION = "1.1.1";
        private int TEXT_OFFSET_CONSTANT = 16;
        private int TEXT_OFFSET_BACKGROUND_PADDING = 5;

        private Graphics graphics;
        private Font font;
        private SolidBrush background_brush;
        private SolidBrush transparency_brush;
        private SolidBrush brush;
        private StringFormat draw_format;

        private string[] measurements = new string[] { "in", "cm", "px" };
        private int current_measurement = Properties.Settings.Default.current_measurement;

        public Form1()
        {
            InitializeComponent();
            font = new Font("Courier New", 16);
            background_brush = new SolidBrush(this.BackColor);
            transparency_brush = new SolidBrush(this.TransparencyKey);
            brush = new SolidBrush(this.ForeColor);
            draw_format = new StringFormat();
        }

        private void draw_horizontal_markers(int adjusted_ppi, int unit_multiplier, int marker_height, int marker_width, int submarker_height, int submarker_width)
        {
            int rect_width = ClientRectangle.Width;
            int units = (int)Math.Ceiling((double)rect_width / adjusted_ppi);

            graphics.FillRectangle(background_brush, new Rectangle(0, ClientRectangle.Height - marker_height, rect_width, marker_height));

            for (int i = 0; i < units; i++)
            {
                int x = (int)(i * adjusted_ppi);
                int text_offset = (TEXT_OFFSET_CONSTANT * (i * unit_multiplier).ToString().Length);

                graphics.FillRectangle(brush, new Rectangle(x, ClientRectangle.Height - marker_height, marker_width, marker_height));


                // Draw submarkers
                for (int j = 0; j < 10; j++)
                {
                    graphics.FillRectangle(brush, new Rectangle((int)(x + (j * Math.Round((double)adjusted_ppi / 10))), ClientRectangle.Height - submarker_height, submarker_width, submarker_height));
                }

                graphics.DrawString((i * unit_multiplier).ToString(), font, brush, new Point(x - text_offset, ClientRectangle.Height - marker_height));
            }
        }

        private void draw_vertical_markers(int adjusted_ppi, int unit_multiplier, int marker_height, int marker_width, int submarker_height, int submarker_width)
        {
            int rect_height = ClientRectangle.Height;
            int units_vertical = (int)Math.Ceiling((double)rect_height / adjusted_ppi);

            graphics.FillRectangle(background_brush, new Rectangle(ClientRectangle.Width - marker_height, 0, marker_height, rect_height));

            for (int i = 0; i < units_vertical; i++)
            {
                int y = (int)(i * adjusted_ppi);
                int text_offset = (TEXT_OFFSET_CONSTANT * (i * unit_multiplier).ToString().Length) + TEXT_OFFSET_CONSTANT;

                graphics.FillRectangle(brush, new Rectangle(ClientRectangle.Width - marker_height, y, marker_height, marker_width));
                // Draw submarkers
                for (int j = 0; j < 10; j++)
                {
                    graphics.FillRectangle(brush, new Rectangle(ClientRectangle.Width - submarker_height, (int)(y + (j * Math.Round((double)adjusted_ppi / 10))), submarker_height, submarker_width));
                }

                // Background in case text is too long to fit
                graphics.FillRectangle(background_brush, new Rectangle(ClientRectangle.Width - text_offset - TEXT_OFFSET_BACKGROUND_PADDING, y - TEXT_OFFSET_CONSTANT - TEXT_OFFSET_BACKGROUND_PADDING, text_offset - marker_height + TEXT_OFFSET_BACKGROUND_PADDING, TEXT_OFFSET_CONSTANT + TEXT_OFFSET_BACKGROUND_PADDING));
                graphics.DrawString((i * unit_multiplier).ToString(), font, brush, new Point(ClientRectangle.Width - text_offset - TEXT_OFFSET_BACKGROUND_PADDING, y - TEXT_OFFSET_CONSTANT - TEXT_OFFSET_BACKGROUND_PADDING));
            }
        }

        private void draw_diagonal_marker(int adjusted_ppi, int unit_multiplier, int marker_height, int marker_width)
        {
            int width = ClientRectangle.Width - marker_height;
            int height = ClientRectangle.Height - marker_height;

            double units_diagonal = Math.Round((Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2)) / adjusted_ppi) * unit_multiplier, 2);

            // Draw highlights before actual lines

            graphics.DrawLine(new Pen(background_brush, marker_width * 2), new Point(0, 0), new Point(width, height));
            graphics.DrawLine(new Pen(brush, marker_width), new Point(0, 0), new Point(width, height));

            graphics.FillRectangle(background_brush, new Rectangle((width / 2) + 10, (height / 2) - 20, TEXT_OFFSET_CONSTANT * (units_diagonal.ToString().Length), TEXT_OFFSET_CONSTANT + 5));
            graphics.DrawString(units_diagonal.ToString(), font, brush, new Point((width / 2) + 10, (height / 2) - 20));
        }

        private void draw()
        {
            graphics = this.CreateGraphics();

            // Fill background
            graphics.FillRectangle(transparency_brush, new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height));

            // Calculate ppi
            int adjusted_ppi = (int) GetDpiForSystem();
            int unit_multiplier = 1;

            if (measurements[current_measurement] == "cm")
            {
                adjusted_ppi = (int)Math.Round(adjusted_ppi / 2.54);
            } else if (measurements[current_measurement] == "px")
            {
                adjusted_ppi = 100;
                unit_multiplier = 100;
            }

            int marker_height = 40;
            int marker_width = 3;
            int submarker_height = 20;
            int submarker_width = 1;

            draw_vertical_markers(adjusted_ppi, unit_multiplier, marker_height, marker_width, submarker_height, submarker_width);

            draw_horizontal_markers(adjusted_ppi, unit_multiplier, marker_height, marker_width, submarker_height, submarker_width);

            draw_diagonal_marker(adjusted_ppi, unit_multiplier, marker_height, marker_width);

            // Cover overlapping markers
            graphics.FillRectangle(brush, new Rectangle(ClientRectangle.Width - marker_height, ClientRectangle.Height - marker_height, marker_height, marker_height));
            graphics.DrawString(measurements[current_measurement], font, background_brush, new Point(ClientRectangle.Width - marker_height, ClientRectangle.Height - marker_height));

            // Draw borders
            graphics.FillRectangle(brush, new Rectangle(0, 0, ClientRectangle.Width - marker_height, 1));
            graphics.FillRectangle(brush, new Rectangle(0, 0, 1, ClientRectangle.Height - marker_height));

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            draw();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void Form1_Click(object sender, EventArgs e)
        {
            if (current_measurement == measurements.Length - 1)
            {
                current_measurement = 0;
            }
            else
            {
                current_measurement++;
            }

            Properties.Settings.Default.current_measurement = current_measurement;
            Properties.Settings.Default.Save();
            this.Invalidate();
        }
    }
}
