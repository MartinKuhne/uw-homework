# data.R
# Author: Martin Kuhne
# Summary: Helper function to retrieve energy data used in various plots
# cf. https://class.coursera.org/exdata-007/

library("data.table")
library("dplyr")
library("lubridate")

# getEnergyData - return a tidyed subset of the energy data
getEnergyData <- function ()
{
    setwd("~/datascience/4.exploratory/ExData_Plotting1")
    
    # force reading columns as "character" as fread will promote to character
    # mid-import when it sees the first '?'
    
    colClasses <- c("character",
                    "character",
                    "character",
                    "character",
                    "character",
                    "character",
                    "character",
                    "character",
                    "character")
    raw <- fread("../household_power_consumption.txt", colClasses = colClasses)
    
    # We will only be using data from the dates 2007-02-01 and 2007-02-02
    tidy <- filter(raw, Date == "1/2/2007" | Date == "2/2/2007")
    
    # there should be a cleaner way to convert these columns into numbers
    tidy <- mutate(tidy, GlobalActivePower = as.numeric(Global_active_power))
    tidy <- mutate(tidy, GlobalReactivePower = as.numeric(Global_reactive_power))
    tidy <- mutate(tidy, Volts = as.numeric(Voltage))
    tidy <- mutate(tidy, GlobalIntensity = as.numeric(Global_intensity))
    tidy <- mutate(tidy, SubMetering1 = as.numeric(Sub_metering_1))
    tidy <- mutate(tidy, SubMetering2 = as.numeric(Sub_metering_2))
    tidy <- mutate(tidy, SubMetering3 = as.numeric(Sub_metering_3))
    
    # convert date and time from string to date format
    tidy <- mutate(tidy, dateTime = 
                   parse_date_time(paste(Date, Time, sep = " "), "%d%m%y %H%M%S"))
    tidy
}