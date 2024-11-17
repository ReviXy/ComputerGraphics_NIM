#include <GL/glew.h>
#include <SFML/Graphics.hpp>
#include <SFML/OpenGL.hpp>
#include <SFML/Window.hpp>
#include <iostream>
#include <iomanip>

// ID шейдерной программы
GLuint Program;
// ID атрибута
GLint Attrib_vertex;
// ID Vertex Buffer Object
GLuint VBO;

struct Vertex {
    GLfloat x;
    GLfloat y;
};

// Исходный код вершинного шейдера
const char* VertexShaderSource = R"(
 #version 330 core
 in vec2 coord;
 void main() {
    vec2 a = coord.yx;
    gl_Position = vec4(a, 0.0, 1.0);
 }
)";

// Исходный код фрагментного шейдера
const char* FragShaderSource = R"(
 #version 330 core
 out vec4 color;
 void main() {
    color = vec4(0, 1, 0, 1);
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
    Vertex triangle[3] = {
     { -1.0f, -1.0f },
     { 0.0f, 1.0f },
     { 1.0f, -1.0f }
    };
    // Передаем вершины в буфер
    glBindBuffer(GL_ARRAY_BUFFER, VBO);
    glBufferData(GL_ARRAY_BUFFER, sizeof(triangle), triangle, GL_STATIC_DRAW);
    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной
    // Проверка ошибок OpenGL, если есть, то вывод в консоль тип ошибки
}

void InitShader() {
    // Создаем вершинный шейдер
    GLuint vShader = glCreateShader(GL_VERTEX_SHADER);
    // Передаем исходный код
    glShaderSource(vShader, 1, &VertexShaderSource, NULL);
    // Компилируем шейдер
    glCompileShader(vShader);
    std::cout << "vertex shader \n";
    // Функция печати лога шейдера
    ShaderLog(vShader); //Пример функции есть в лабораторной

    // Создаем фрагментный шейдер
    GLuint fShader = glCreateShader(GL_FRAGMENT_SHADER);
    // Передаем исходный код
    glShaderSource(fShader, 1, &FragShaderSource, NULL);
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
    glEnableVertexAttribArray(Attrib_vertex); // Включаем массив атрибутов
    glBindBuffer(GL_ARRAY_BUFFER, VBO); // Подключаем VBO
    // сообщаем OpenGL как он должен интерпретировать вершинные данные.
    glVertexAttribPointer(Attrib_vertex, 2, GL_FLOAT, GL_FALSE, 0, 0);
    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO
    glDrawArrays(GL_TRIANGLES, 0, 3); // Передаем данные на видеокарту(рисуем)

    glDisableVertexAttribArray(Attrib_vertex); // Отключаем массив атрибутов
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

int main() {
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
