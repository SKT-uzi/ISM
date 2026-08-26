// Define Chart
function charts(id, current, item, theme) {
  let obj = echarts.getInstanceByDom(document.getElementById(id));
  if (!obj) {
    obj = echarts.init(document.getElementById(id));
  }
  const defaultColor = "#0EC3A4";
  const dangerColor = "#E30327";
  const defaultLineColor = "rgba(14, 195, 164, 0.15)";
  const dangerLineColor = "rgba(227, 3, 39, 0.15)";
  const barColor = current > 90 ? dangerColor : defaultColor;
  const lineColor = theme === "dark" ? "rgba(0, 0, 0, 0.4)" : current > 90 ? dangerLineColor : defaultLineColor;
  const titleColor = theme === "dark" ? "#e5e5e5" : "#7B7B7B";
  const detailColor = theme === "dark" ? "#fff" : "#55565A";
  const options = {
    series: [
      {
        type: 'gauge',
        startAngle: 90,
        endAngle: -270,
        min: 0,
        max: 100,
        axisLine: {
          lineStyle: {
            width: 14,
            color: [
              [1, lineColor]
            ]
          }
        },
        progress: {
          show: true,
          overlap: false,
          roundCap: true,
          clip: false,
          itemStyle: {
            color: barColor,
          }
        },
        pointer: {
          show: false
        },
        axisTick: {
          show: false
        },
        splitLine: {
          show: false
        },
        axisLabel: {
          show: false
        },
        title: {
          fontFamily: 'Roboto',
          fontSize: 14,
          color: titleColor,
          offsetCenter: [0, '22%']
        },
        detail: {
          fontFamily: 'Roboto',
          fontSize: 32,
          color: detailColor,
          offsetCenter: [0, '-5%'],
          valueAnimation: true,
          formatter: function (value) {
            return Math.round(value) + '%';
          },
        },
        data: [
          {
            value: current,
            name: item
          }
        ]
      }
    ]
  };
  obj.setOption(options);
}