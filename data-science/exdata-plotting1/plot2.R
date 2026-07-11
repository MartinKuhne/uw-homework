# plot2.R
# Author: Martin Kuhne
# Summary: plot for week 1 assignment
# cf. https://class.coursera.org/exdata-007/

source("data.R")

plot2 <- function()
{
    data <- getEnergyData()

    # creating a type "n" plot does not plot data initially
    # then draw the lines we need to imitate the reference plot
    with(data, 
     {
         plot(dateTime, GlobalActivePower, type="n", 
              ylab = "Global Active Power (kilowatts)",
              xlab = "")
         lines(dateTime, GlobalActivePower)
     })
}

plot2()

png(file = "plot2.png")
plot2()
dev.off()
