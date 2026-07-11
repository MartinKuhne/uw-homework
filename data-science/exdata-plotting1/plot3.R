# plot3.R
# Author: Martin Kuhne
# Summary: plot for week 1 assignment
# cf. https://class.coursera.org/exdata-007/

source("data.R")

plot3 <- function()
{    
    data <- getEnergyData()
    with(data, 
    {
        plot(dateTime, SubMetering1, type="n", 
             ylab = "Energy sub metering",
             xlab = "")
        lines(dateTime, SubMetering1, col="black")
        lines(dateTime, SubMetering2, col="red")
        lines(dateTime, SubMetering3, col="blue")
        legend("topright", lty=1,
               c("Sub_metering_1", "Sub_metering_2", "Sub_metering_3"),
               col=c("black", "red", "blue"))
    })
}

plot3()

png(file = "plot3.png")
plot3()
dev.off()
