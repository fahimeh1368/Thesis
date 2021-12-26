source("~/GitHub/SofiaWL/ReplicationPackage/WorkloadMeasures/CumulativePercentageLib.r")

#Compare the median reviewer to the reviewer at 95th percentile

#per month M
ORM <- read.csv("~/GitHub/SofiaWL/ReplicationPackage/Data/Workload/Actual/OpenReviewsPerMonth.csv")
summary(ORM)
quantile_all(ORM, .50)
quantile_all(ORM, .95)

#per week W
ORW <- read.csv("~/GitHub/SofiaWL/ReplicationPackage/Data/Workload/Actual/OpenReviewsPerWeek.csv")
quantile_all(ORW, .50)
quantile_all(ORW, .95)

# per day D
ORD <- read.csv("~/GitHub/SofiaWL/ReplicationPackage/Data/Workload/Actual/OpenReviewsPerDay.csv")
summary(ORD)
quantile_all(ORD, .50)
quantile_all(ORD, .95)


# plot in paper of actual workload of each project
#pdf("~/Downloads/WorkloadActual.pdf")
plot(seq(1:100), seq(1:100), type = 'l', lwd = 3, lty = 1, ylab = "Cumulative Percentage of Reviews", xlab = "Percentage of Reviewers")
plot_line_percentage_percentage(percentage_percentage_freq(openA$CoreFX), lty = 1)
plot_line_percentage_percentage(percentage_percentage_freq(openA$CoreCLR), lty = 2)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Roslyn), lty = 3)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Rust), lty = 4)
plot_line_percentage_percentage(percentage_percentage_freq(openA$Kubernetes), lty = 5)
abline(v = 20)
abline(h = 80)
abline(h = 20)
legend("bottomright", legend=c("CoreFX", "CoreCLR", "Roslyn", "Rust", "Kubernetes"), lty=c(1:5), lwd = 3, cex=1.2)
#dev.off()

