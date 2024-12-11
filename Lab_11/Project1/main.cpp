#include <GL/glew.h>
#include <SFML/Graphics.hpp>
#include <SFML/OpenGL.hpp>
#include <SFML/Window.hpp>
#include <iostream>
#include <iomanip>
#include <vector>

// ID шейдерной программы
GLuint Program;
// ID атрибута
GLint Attrib_vertex;
GLint v_color_id;
// ID Vertex Buffer Object
GLuint VBO;
GLuint CBO;

struct Vertex {
    GLfloat x;
    GLfloat y;
};

struct Color {
    GLfloat r;
    GLfloat g;
    GLfloat b;
    GLfloat a;
};

enum Shape {
    Triangle,
    Quadrangle,
    Fan,
    Pentagon
};

enum Fill {
    Hardcode,
    Uniform,
    Gradient
};

Shape shape;
Fill fill;

std::vector<Vertex> verteces;
Color u_color = {1, 0, 0, 1};
std::vector<Color> gradient_colors;

const GLfloat Pi = 3.14159274101257324219f;

// Исходный код вершинного шейдера
const char* VertexShaderSource = R"(
 #version 330 core
 in vec2 coord;
 void main() {
    gl_Position = vec4(coord, 0.0, 1.0);
 }
)";

const char* GradientVertexShaderSource = R"(
 #version 330 core
 in vec2 coord;
 in vec4 vertexColor;
 out vec4 v_color;

 void main() {
    gl_Position = vec4(coord, 0.0, 1.0);
    v_color = vertexColor;
 }
)";

//_________________________________________

// Исходный код фрагментного шейдера
const char* FragShaderSource = R"(
 #version 330 core
 out vec4 color;
 void main() {
    color = vec4(0, 1, 0, 1);
 }
)";

const char* UniformFragShaderSource = R"(
 #version 330 core
 uniform vec4 u_color;
 out vec4 color;
 void main() {
    color = u_color;
 }
)";

const char* GradientFragShaderSource = R"(
 #version 330 core
 in vec4 v_color;
 out vec4 color;
 void main() {
    color = v_color;
 }
)";

void checkOpenGLerror() {
    GLenum err;
    while ((err = glGetError()) != GL_NO_ERROR)
    {
        std::cout << "Error! Code: " << std::hex << err << std::dec << std::endl;
    }
}

void ShaderLog(unsigned int shader)
{
    int infologLen = 0;
    glGetShaderiv(shader, GL_INFO_LOG_LENGTH, &infologLen);
    if (infologLen > 1)
    {
        int charsWritten = 0;
        std::vector<char> infoLog(infologLen);
        glGetShaderInfoLog(shader, infologLen, &charsWritten, infoLog.data());
        std::cout << "InfoLog: " << infoLog.data() << std::endl;
    }
}

void InitVBO() {
    glGenBuffers(1, &VBO);
    // Вершины нашего треугольника

    // Передаем вершины в буфер
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, verteces.size() * 2 * sizeof(GLfloat), verteces.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);

    glGenBuffers(1, &CBO);
    glBindBuffer(GL_ARRAY_BUFFER, CBO);
    glBufferData(GL_ARRAY_BUFFER, gradient_colors.size() * 4 * sizeof(GLfloat), gradient_colors.data(), GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);

    checkOpenGLerror(); //Пример функции есть в лабораторной
    // Проверка ошибок OpenGL, если есть, то вывод в консоль тип ошибки
}

void InitShader() {
    // Создаем вершинный шейдер
    GLuint vShader = glCreateShader(GL_VERTEX_SHADER);
    // Передаем исходный код
    
    if (fill == Gradient)
        glShaderSource(vShader, 1, &GradientVertexShaderSource, NULL);
    else 
        glShaderSource(vShader, 1, &VertexShaderSource, NULL);

    // Компилируем шейдер
    glCompileShader(vShader);
    std::cout << "vertex shader \n";
    // Функция печати лога шейдера
    ShaderLog(vShader); //Пример функции есть в лабораторной

    // Создаем фрагментный шейдер
    GLuint fShader = glCreateShader(GL_FRAGMENT_SHADER);
    // Передаем исходный код

    switch (fill) {
    case Hardcode: glShaderSource(fShader, 1, &FragShaderSource, NULL); break;
    case Uniform: glShaderSource(fShader, 1, &UniformFragShaderSource, NULL); break;
    case Gradient: glShaderSource(fShader, 1, &GradientFragShaderSource, NULL); break;
    default:break;
    }
    
    // Компилируем шейдер
    glCompileShader(fShader);
    std::cout << "fragment shader \n";
    // Функция печати лога шейдера
    ShaderLog(fShader);

    // Создаем программу и прикрепляем шейдеры к ней
    Program = glCreateProgram();
    glAttachShader(Program, vShader);
    glAttachShader(Program, fShader);
    // Линкуем шейдерную программу
    glLinkProgram(Program);
    // Проверяем статус сборки
    int link_ok;
    glGetProgramiv(Program, GL_LINK_STATUS, &link_ok);
    if (!link_ok) {
        std::cout << "error attach shaders \n";
        return;
    }

    // Вытягиваем ID атрибута из собранной программы
    const char* attr_name = "coord"; //имя в шейдере
    Attrib_vertex = glGetAttribLocation(Program, attr_name);
    if (Attrib_vertex == -1) {
        std::cout << "could not bind attrib " << attr_name << std::endl;
        return;
    }

    checkOpenGLerror();
}

void Init() {
    // Шейдеры
    InitShader();
    // Вершинный буфер
    InitVBO();
}

void Draw() {
    glUseProgram(Program); // Устанавливаем шейдерную программу текущей

    if (fill == Uniform) {
        GLuint u_color_id = glGetUniformLocation(Program, "u_color");
        glUniform4f(u_color_id, u_color.r, u_color.g, u_color.b, u_color.a);
    }

    glEnableVertexAttribArray(Attrib_vertex); // Включаем массив атрибутов
    glBindBuffer(GL_ARRAY_BUFFER, VBO); // Подключаем VBO
    // сообщаем OpenGL как он должен интерпретировать вершинные данные.
    glVertexAttribPointer(Attrib_vertex, 2, GL_FLOAT, GL_FALSE, 0, 0);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO

    if (fill == Gradient) {
        v_color_id = glGetAttribLocation(Program, "vertexColor");
        glEnableVertexAttribArray(v_color_id);
        glBindBuffer(GL_ARRAY_BUFFER, CBO);
        glVertexAttribPointer(v_color_id, 4, GL_FLOAT, GL_FALSE, 0, 0);

        glBindBuffer(GL_ARRAY_BUFFER, 0);
    }
    

    // Передаем данные на видеокарту(рисуем)
    switch (shape) {
    case Triangle: glDrawArrays(GL_TRIANGLES, 0, verteces.size()); break;
    case Fan: glDrawArrays(GL_TRIANGLE_FAN, 0, verteces.size()); break;
    case Quadrangle: glDrawArrays(GL_QUADS, 0, verteces.size()); break;
    case Pentagon: glDrawArrays(GL_TRIANGLE_FAN, 0, verteces.size()); break;
    default: break;
    }

    glDisableVertexAttribArray(Attrib_vertex); // Отключаем массив атрибутов
    if (fill == Gradient)
        glDisableVertexAttribArray(v_color_id);
    glUseProgram(0); // Отключаем шейдерную программу

    checkOpenGLerror();
}

// Освобождение буфера
void ReleaseVBO() {
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    glDeleteBuffers(1, &VBO);
}

// Освобождение шейдеров
void ReleaseShader() {
    // Передавая ноль, мы отключаем шейдерную программу
    glUseProgram(0);
    // Удаляем шейдерную программу
    glDeleteProgram(Program);
}

void Release() {
    // Шейдеры
    ReleaseShader();
    // Вершинный буфер
    ReleaseVBO();
}

std::vector<Color> GenerateColors(int n) {
    std::vector<Color> res;
    for (int i = 0; i < n; i++)
        res.push_back({ (GLfloat)std::rand() / RAND_MAX, (GLfloat)std::rand() / RAND_MAX, (GLfloat)std::rand() / RAND_MAX, 1 });
    
    return res;
}

int main() {
    shape = Quadrangle;
    fill = Gradient;

    u_color = {1, 0, 1, 1};

    switch (shape)
    {
    case Triangle:
        verteces = {
            { -1.0f, -1.0f },
            { 0.0f, 1.0f },
            { 1.0f, -1.0f }
        };
        if (fill == Gradient) gradient_colors = GenerateColors(3);
        break;
    case Fan:
        verteces.push_back({ 0.0f, 0.0f });
        for (GLfloat angle = 15.0f; angle <= 165.0f; angle += 30.0f)
            verteces.push_back({ cos(angle / 180.0f * Pi), sin(angle / 180.0f * Pi) });
        if (fill == Gradient) gradient_colors = GenerateColors(7);
        break;
    case Quadrangle:
        verteces = {
            { -1.0f, 1.0f },
            { 0.0f, 1.0f },
            { 1.0f, -1.0f },
            { 0.0f, -1.0f }
        };
        if (fill == Gradient) gradient_colors = GenerateColors(4);
        break;
    case Pentagon:
        verteces.push_back({ 0.0f, 0.0f });
        for (GLfloat angle = 0.0f; angle <= 360.0f; angle += 72.0f)
            verteces.push_back({ cos(angle / 180.0f * Pi), sin(angle / 180.0f * Pi) });
        if (fill == Gradient) {
            gradient_colors = GenerateColors(6);
            gradient_colors.push_back(gradient_colors[1]);
        }
        break;
    default: break;
    }
    
    sf::Window window(sf::VideoMode(600, 600), "My OpenGL window", sf::Style::Default, sf::ContextSettings(24));
    window.setVerticalSyncEnabled(true);
    window.setActive(true);
    glewInit();
    Init();
    while (window.isOpen()) {
        sf::Event event;
        while (window.pollEvent(event)) {
            if (event.type == sf::Event::Closed) { window.close(); break; }
            else if (event.type == sf::Event::Resized) { glViewport(0, 0, event.size.width, event.size.height); }
        }
        if (!window.isOpen()) continue;
        glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
        Draw();
        window.display();
    }
    Release();
    return 0;
}
