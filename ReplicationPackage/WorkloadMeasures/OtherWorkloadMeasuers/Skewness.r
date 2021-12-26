install.packages("e1071")
library(e1071)    

percentage_difference <- function(a, s) {
  ((s/a)-1) *100
}

yule <- function(x) {
  quantile_range = IQR(x)/2
  (quantile_range - median(x)) / quantile_range
}

quantiles_list <- function(x) {
  quantile(na.omit(x),probs=seq(0,1,.05))
}

OpenRevs <- read.csv("~/Downloads/OpenRevs/OpenReviewsPerQuarter.csv")
SimOpenRevs <- read.csv("~/Downloads/OpenRevs/OpenReviewscHRevPerQuarter.csv")
#OpenRevs <- read.csv("~/Downloads/NumReviewers/NumberOfReviewersQuarter.csv")


vioplot2log( 
  na.omit(OpenRevs$CoreFX),
  na.omit(OpenRevs$CoreCLR),
  na.omit(OpenRevs$Roslyn),
  na.omit(OpenRevs$Rust),
  na.omit(OpenRevs$Kubernetes),
  side = 'left', col = 'white',
  names = c( "CoreFX","CoreCLR", "Roslyn", "Rust", "Kubernetes")
)

summary(OpenRevs)
summary(SimOpenRevs)

quantiles_list(na.omit(OpenRevs$CoreFX))
quantiles_list(na.omit(SimOpenRevs$CoreFX))

quantiles_list(na.omit(OpenRevs$CoreCLR))
quantiles_list(na.omit(SimOpenRevs$CoreCLR))

quantiles_list(na.omit(OpenRevs$Roslyn))
quantiles_list(na.omit(SimOpenRevs$Roslyn))

quantiles_list(na.omit(OpenRevs$Rust))
quantiles_list(na.omit(SimOpenRevs$Rust))

quantiles_list(na.omit(OpenRevs$Kubernetes))
quantiles_list(na.omit(SimOpenRevs$Kubernetes))

percentile = .50
percentage_difference(quantile(na.omit(OpenRevs$CoreFX) , percentile, names  = FALSE),quantile(na.omit(SimOpenRevs$CoreFX) , percentile, names  = FALSE))
percentage_difference(quantile(na.omit(OpenRevs$CoreCLR) , percentile, names  = FALSE),quantile(na.omit(SimOpenRevs$CoreCLR) , percentile, names  = FALSE))
percentage_difference(quantile(na.omit(OpenRevs$Roslyn) , percentile, names  = FALSE),quantile(na.omit(SimOpenRevs$Roslyn) , percentile, names  = FALSE))
percentage_difference(quantile(na.omit(OpenRevs$Rust) , percentile, names  = FALSE),quantile(na.omit(SimOpenRevs$Rust) , percentile, names  = FALSE))
percentage_difference(quantile(na.omit(OpenRevs$Kubernetes) , percentile, names  = FALSE),quantile(na.omit(SimOpenRevs$Kubernetes) , percentile, names  = FALSE))

plot(density(SimOpenRevs$Kubernetes))
plot(ecdf(1-SimOpenRevs$KubernetesSum))
hist(OpenRevs$Kubernetes)



kurtosis(na.omit(openA$CoreFX))
kurtosis(na.omit(OpenRevs$CoreFX))
kurtosis(na.omit(OpenRevs$CoreCLR))
kurtosis(na.omit(OpenRevs$Roslyn))
kurtosis(na.omit(OpenRevs$Rust))

kurtosis(na.omit(openA$Kubernetes))
kurtosis(na.omit(openOwnRec$Kubernetes))
kurtosis(na.omit(openWhoDo$Kubernetes))

skewness(na.omit(OpenRevs$CoreFX))
skewness(na.omit(OpenRevs$CoreCLR))
skewness(na.omit(OpenRevs$Roslyn))
skewness(na.omit(OpenRevs$Rust))
skewness(na.omit(OpenRevs$Kubernetes))

skewness(na.omit(SimOpenRevs$CoreFX))
skewness(na.omit(SimOpenRevs$CoreCLR))
skewness(na.omit(SimOpenRevs$Roslyn))
skewness(na.omit(SimOpenRevs$Rust))
skewness(na.omit(SimOpenRevs$Kubernetes))

percentage_difference(skewness(na.omit(OpenRevs$CoreFX)),skewness(na.omit(SimOpenRevs$CoreFX)))
percentage_difference(skewness(na.omit(OpenRevs$CoreCLR)),skewness(na.omit(SimOpenRevs$CoreCLR)))
percentage_difference(skewness(na.omit(OpenRevs$Roslyn)),skewness(na.omit(SimOpenRevs$Roslyn)))
percentage_difference(skewness(na.omit(OpenRevs$Rust)),skewness(na.omit(SimOpenRevs$Rust)))
percentage_difference(skewness(na.omit(OpenRevs$Kubernetes)),skewness(na.omit(SimOpenRevs$Kubernetes)))

percentage_difference(kurtosis(na.omit(OpenRevs$CoreFX)),kurtosis(na.omit(SimOpenRevs$CoreFX)))
percentage_difference(kurtosis(na.omit(OpenRevs$CoreCLR)),kurtosis(na.omit(SimOpenRevs$CoreCLR)))
percentage_difference(kurtosis(na.omit(OpenRevs$Roslyn)),kurtosis(na.omit(SimOpenRevs$Roslyn)))
percentage_difference(kurtosis(na.omit(OpenRevs$Rust)),kurtosis(na.omit(SimOpenRevs$Rust)))
percentage_difference(kurtosis(na.omit(OpenRevs$Kubernetes)),kurtosis(na.omit(SimOpenRevs$Kubernetes)))

#only looks at Q1 to Q3 and compares with Q2, so misses the outliers 
percentage_difference(yule(na.omit(OpenRevs$CoreFX)),yule(na.omit(SimOpenRevs$CoreFX)))
percentage_difference(yule(na.omit(OpenRevs$CoreCLR)),yule(na.omit(SimOpenRevs$CoreCLR)))
percentage_difference(yule(na.omit(OpenRevs$Roslyn)),yule(na.omit(SimOpenRevs$Roslyn)))
percentage_difference(yule(na.omit(OpenRevs$Rust)),yule(na.omit(SimOpenRevs$Rust)))
percentage_difference(yule(na.omit(OpenRevs$Kubernetes)),yule(na.omit(SimOpenRevs$Kubernetes)))


