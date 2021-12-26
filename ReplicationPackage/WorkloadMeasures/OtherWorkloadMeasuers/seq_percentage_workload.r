cumA <- percentage_percentage_freq(openA$Roslyn)
cumWhoDo <- percentage_percentage_freq(openWhoDo$Roslyn)

s = seq(0, 100, .25)
s
#2
pA = get_cumulative_at_df (cumA, s)
pWhoDo = get_cumulative_at_df (cumA, s)

plot(s, pA, type = 'l', col = 'black', pch = 22, lty = 1, lwd = 3)
lines(s, pWhoDo, type = 'l', col = 'blue', pch = 2,lty = 2, lwd = 3)

plot_percentage_freqency(openA$CoreFX)
plot_line_percentage_freqency(openA$Rust, lty = 2)

pdf("~/Downloads/WorkloadActual.pdf")
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
dev.off()

get_cumulative_at_df(percentage_percentage_freq(openA$CoreFX), 20);
get_cumulative_at_df(percentage_percentage_freq(openWhoDo$CoreFX), 20);
get_cumulative_at_df(percentage_percentage_freq(openRet$CoreFX), 20);
