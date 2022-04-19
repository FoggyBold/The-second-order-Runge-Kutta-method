import json
import matplotlib.pyplot as plt

def lineplot(x_data, y_data, z_data, x_data2, y_data2, z_data2, x_data3, y_data3, z_data3):
    _, ax = plt.subplots()
    
    plt.plot(x_data, y_data, 'm')
    plt.plot(x_data, z_data, 'b')
    plt.plot(x_data2, y_data2, 'g')
    plt.plot(x_data2, z_data2, 'k')
    plt.plot(x_data3, y_data3, 'y')
    plt.plot(x_data3, z_data3, 'r')
    plt.show()


with open("D:\\лабы\\6 семестр\\ЧМ\\laba2.2\\Save\\temp.json", "r") as read_file:
    data = json.load(read_file)
lineplot(data['X1'], data['Y1'], data['Z1'], data['X2'], data['Y2'], data['Z2'], data['X3'], data['Y3'], data['Z3'])