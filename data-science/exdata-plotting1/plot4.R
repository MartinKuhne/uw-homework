# plot4.R
# Author: Martin Kuhne
# Summary: plot for week 1 assignment
# cf. https://class.coursera.org/exdata-007/

source("data.R")

plot4 <- function()
{
    data <- getEnergyData()
    
    # configure a 2 by 2 set of plots, by column
    par(mfcol = c(2,2))

    # add the two previous plots
    plot2()
    plot3()
    
    # add the two plots new to this 2x2    
    with(data, 
    {
        plot(dateTime, Volts, type="n", 
             ylab = "Voltage",
             xlab = "datetime")
        lines(dateTime, Volts, col="black")
        plot(dateTime, GlobalReactivePower, type="n", 
             ylab = "Global_reactive_power",
             xlab = "datetime")
        lines(dateTime, GlobalReactivePower, col="black")
    })
}

plot4()

png(file = "plot4.png")
plot4()
dev.off()
