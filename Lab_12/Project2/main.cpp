#include <GL/glew.h>
#include <SFML/Graphics.hpp>
#include <SFML/OpenGL.hpp>
#include <SFML/Window.hpp>
#include <iostream>
#include <iomanip>
#include <cmath>
#include "SOIL.h"

// ID шейдерной программы
GLuint Program;
// ID атрибута
GLint coordAttribID;
GLint colorAttribID;
GLint textureCoordAttribID;

GLint offsetUniformID;
GLint texture1UniformID;
GLint texture2UniformID;
GLint proportionUniformID;
// ID Vertex Buffer Object
GLuint VBO;

struct Vertex {
    GLfloat x;
    GLfloat y;
    GLfloat z;
};

struct Color {
    GLfloat r;
    GLfloat g;
    GLfloat b;
};

struct TextureCoordinate {
    GLfloat x;
    GLfloat y;
};

struct VertexColor {
    Vertex v;
    Color c;
};

struct VertexColorTexture {
    Vertex v;
    Color c;
    TextureCoordinate t;
};

const GLfloat Pi = 3.14159274101257324219f;

Vertex offset = { 0.0f, 0.0f, 0.0f };
float proportion = 0.5f;
Vertex scale = { 0.5f, 0.5f, 1.0f };

// Исходный код вершинного шейдера
const char* VertexShaderSource = R"(
 #version 330 core
 uniform vec3 offset;
 in vec3 coord;
 in vec3 color;
 in vec2 texCoord;
 out vec3 vertexColor; 
 out vec2 TexCoord; 
 uniform vec3 scale;

 void main() {
    gl_Position = vec4((scale * coord) + offset, 1.0f);
    vertexColor = color;
    TexCoord = vec2(texCoord.x, 1.0f - texCoord.y);
 }
)";

const char* VertexShaderSource1 = R"(
 #version 330 core
 uniform vec3 offset;
 in vec3 coord;
 in vec3 color;
 in vec2 texCoord;
 out vec3 vertexColor; 
 out vec2 TexCoord; 

 void main() {
    gl_Position = vec4(coord + offset, 1.0f);
    vertexColor = color;
    TexCoord = vec2(texCoord.x, 1.0f - texCoord.y);
 }
)";

// Исходный код фрагментного шейдера
const char* FragShaderSource = R"(
 #version 330 core
 in vec3 vertexColor;
 in vec2 TexCoord;

 out vec4 color;
 
 uniform float proportion;
 uniform sampler2D ourTexture1;
 uniform sampler2D ourTexture2;

 void main() {
    color = vec4(vertexColor, 1.0f);
 }
)";

const char* FragShaderSource1 = R"(
 #version 330 core
 in vec3 vertexColor;
 in vec2 TexCoord;

 out vec4 color;
 
 uniform float proportion;
 uniform sampler2D ourTexture1;
 uniform sampler2D ourTexture2;

 void main() {
    // vec4(vertexColor, 1.0f)
    // texture(ourTexture2, TexCoord)
    color = mix(texture(ourTexture1, TexCoord), vec4(vertexColor, 1.0f), proportion);
 }
)";

const char* FragShaderSource2 = R"(
 #version 330 core
 in vec3 vertexColor;
 in vec2 TexCoord;

 out vec4 color;
 
 uniform float proportion;
 uniform sampler2D ourTexture1;
 uniform sampler2D ourTexture2;

 void main() {
    // vec4(vertexColor, 1.0f)
    // texture(ourTexture2, TexCoord)
    color = mix(texture(ourTexture1, TexCoord), texture(ourTexture2, TexCoord), proportion);
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
    VertexColorTexture tetrahedron[12] = {
        {{ 0.0f, 0.0f, 0.0f },   { 1.0f, 0.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 0.5f, 1.0f, 0.0f },   { 0.0f, 1.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 1.0f, 0.0f, 0.0f },   { 0.0f, 0.0f, 1.0f }, {0.0f, 0.0f}},

        {{ 0.0f, 0.0f, 0.0f },   { 1.0f, 0.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 0.5f, 1.0f, 0.0f },   { 0.0f, 1.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 0.5f, 0.5f, -1.0f },  { 1.0f, 1.0f, 1.0f }, {0.0f, 0.0f}},

        {{ 0.5f, 1.0f, 0.0f },   { 0.0f, 1.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 1.0f, 0.0f, 0.0f },   { 0.0f, 0.0f, 1.0f }, {0.0f, 0.0f}},
        {{ 0.5f, 0.5f, -1.0f },  { 1.0f, 1.0f, 1.0f }, {0.0f, 0.0f}},

        {{ 0.0f, 0.0f, 0.0f },   { 1.0f, 0.0f, 0.0f }, {0.0f, 0.0f}},
        {{ 1.0f, 0.0f, 0.0f },   { 0.0f, 0.0f, 1.0f }, {0.0f, 0.0f}},
        {{ 0.5f, 0.5f, -1.0f },  { 1.0f, 1.0f, 1.0f }, {0.0f, 0.0f}}
    };
    
    VertexColorTexture cube[36] = {
        {{0.0f, 0.0f, 0.0f}, {1.0f, 0.0f, 0.0f}, {0.0f, 0.0f}},//1
        {{0.0f, 0.5f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 1.0f}},//2
        {{0.5f, 0.5f, 0.0f}, {0.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//3

        {{0.0f, 0.0f, 0.0f}, {1.0f, 0.0f, 0.0f}, {0.0f, 0.0f}},//1
        {{0.5f, 0.5f, 0.0f}, {0.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//3
        {{0.5f, 0.0f, 0.0f}, {1.0f, 1.0f, 0.0f}, {1.0f, 0.0f}},//4


        {{0.1f, 0.1f, 0.35f}, {0.0f, 1.0f, 1.0f}, {0.0f, 0.0f}},//5
        {{0.0f, 0.0f, 0.0f}, {1.0f, 0.0f, 0.0f}, {0.0f, 1.0f}},//1
        {{0.5f, 0.0f, 0.0f}, {1.0f, 1.0f, 0.0f}, {1.0f, 1.0f}},//4

        {{0.1f, 0.1f, 0.35f}, {0.0f, 1.0f, 1.0f}, {0.0f, 0.0f}},//5
        {{0.5f, 0.0f, 0.0f}, {1.0f, 1.0f, 0.0f}, {1.0f, 1.0f}},//4
        {{0.6f, 0.1f, 0.35f}, {1.0f, 1.0f, 1.0f}, {1.0f, 0.0f}},//8


        {{0.5f, 0.0f, 0.0f}, {1.0f, 1.0f, 0.0f}, {1.0f, 0.0f}},//4
        {{0.5f, 0.5f, 0.0f}, {0.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//3
        {{0.6f, 0.6f, 0.35f}, {0.0f, 0.0f, 0.0f}, {0.0f, 1.0f}},//7

        {{0.5f, 0.0f, 0.0f}, {1.0f, 1.0f, 0.0f}, {1.0f, 0.0f}},//4
        {{0.6f, 0.6f, 0.35f}, {0.0f, 0.0f, 0.0f}, {0.0f, 1.0f}},//7
        {{0.6f, 0.1f, 0.35f}, {1.0f, 1.0f, 1.0f}, {0.0f, 0.0f}},//8


        {{0.1f, 0.1f, 0.35f}, {0.0f, 1.0f, 1.0f}, {1.0f, 0.0f}},//5
        {{0.1f, 0.6f, 0.35f}, {1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//6
        {{0.0f, 0.5f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 1.0f}},//2

        {{0.1f, 0.1f, 0.35f}, {0.0f, 1.0f, 1.0f}, {1.0f, 0.0f}},//5
        {{0.0f, 0.5f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 1.0f}},//2
        {{0.0f, 0.0f, 0.0f}, {1.0f, 0.0f, 0.0f}, {0.0f, 0.0f}},//1


        {{0.0f, 0.5f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 0.0f}},//2
        {{0.1f, 0.6f, 0.35f}, {1.0f, 0.0f, 1.0f}, {0.0f, 1.0f}},//6
        {{0.6f, 0.6f, 0.35f}, {0.0f, 0.0f, 0.0f}, {1.0f, 1.0f}},//7

        {{0.0f, 0.5f, 0.0f}, {0.0f, 1.0f, 0.0f}, {0.0f, 0.0f}},//2
        {{0.6f, 0.6f, 0.35f}, {0.0f, 0.0f, 0.0f}, {1.0f, 1.0f}},//7
        {{0.5f, 0.5f, 0.0f}, {0.0f, 0.0f, 1.0f}, {1.0f, 0.0f}},//3


        {{0.6f, 0.1f, 0.35f}, {1.0f, 1.0f, 1.0f}, {0.0f, 0.0f}},//8
        {{0.6f, 0.6f, 0.35f}, {0.0f, 0.0f, 0.0f}, {0.0f, 1.0f}},//7
        {{0.1f, 0.6f, 0.35f}, {1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//6

        {{0.6f, 0.1f, 0.35f}, {1.0f, 1.0f, 1.0f}, {0.0f, 0.0f}},//8
        {{0.1f, 0.6f, 0.35f}, {1.0f, 0.0f, 1.0f}, {1.0f, 1.0f}},//6
        {{0.1f, 0.1f, 0.35f}, {0.0f, 1.0f, 1.0f}, {1.0f, 0.0f}},//5
    };

    VertexColorTexture hsv_circle[362];
    hsv_circle[0] = { {0.0f, 0.0f, 0.0f}, {1.0f, 1.0f, 1.0f}, {0.0f, 0.0f} };
    for (int i = 0; i < 360; i++) {
        float x = cos((float)i / 180 * Pi);
        float y = sin((float)i / 180 * Pi);

        int Hi = (i / 60) % 6;
        float f = ((float)i / 60) - (i / 60);
        float p = 0;
        float q = 1 - f;
        float t = f;

        float r, g, b;

        switch (Hi)
        {
        case 0: r = 1; g = t; b = p; break;
        case 1: r = q; g = 1; b = p; break;
        case 2: r = p; g = 1; b = t; break;
        case 3: r = p; g = q; b = 1; break;
        case 4: r = t; g = p; b = 1; break;
        case 5: r = 1; g = p; b = q; break;

        default: break;
        }

        hsv_circle[i + 1] = {{ x, y, 0.0f }, { r, g, b }, { 0.0f, 0.0f }};

    }
    hsv_circle[361] = hsv_circle[1];

    // Передаем вершины в буфер
    glBindBuffer(GL_ARRAY_BUFFER, VBO);

    //glBufferData(GL_ARRAY_BUFFER, sizeof(tetrahedron), tetrahedron, GL_STATIC_DRAW);
    glBufferData(GL_ARRAY_BUFFER, sizeof(cube), cube, GL_STATIC_DRAW);
    //glBufferData(GL_ARRAY_BUFFER, sizeof(hsv_circle), hsv_circle, GL_STATIC_DRAW);

    glBindBuffer(GL_ARRAY_BUFFER, 0);
    checkOpenGLerror(); //Пример функции есть в лабораторной
    // Проверка ошибок OpenGL, если есть, то вывод в консоль тип ошибки
}

void InitShader() {
    // Создаем вершинный шейдер
    GLuint vShader = glCreateShader(GL_VERTEX_SHADER);
    // Передаем исходный код
    glShaderSource(vShader, 1, &VertexShaderSource1, NULL);
    // Компилируем шейдер
    glCompileShader(vShader);
    std::cout << "vertex shader \n";
    // Функция печати лога шейдера
    ShaderLog(vShader); //Пример функции есть в лабораторной

    // Создаем фрагментный шейдер
    GLuint fShader = glCreateShader(GL_FRAGMENT_SHADER);
    // Передаем исходный код
    glShaderSource(fShader, 1, &FragShaderSource2, NULL);
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
    coordAttribID = glGetAttribLocation(Program, "coord");
    colorAttribID = glGetAttribLocation(Program, "color");
    textureCoordAttribID = glGetAttribLocation(Program, "texCoord");

    offsetUniformID = glGetUniformLocation(Program, "offset");
    texture1UniformID = glGetUniformLocation(Program, "ourTexture1");
    texture2UniformID = glGetUniformLocation(Program, "ourTexture2");
    proportionUniformID = glGetUniformLocation(Program, "proportion");

    checkOpenGLerror();
}

GLuint texture1;
GLuint texture2;

void Init() {
    // Шейдеры
    InitShader();
    // Вершинный буфер
    InitVBO();

    glEnable(GL_DEPTH_TEST);
    
    glGenTextures(1, &texture1);
    glBindTexture(GL_TEXTURE_2D, texture1);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    int width, height;
    unsigned char* image = SOIL_load_image("88.png", &width, &height, 0, SOIL_LOAD_RGB);
    std::cout << SOIL_last_result() << std::endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);
    
    glGenTextures(1, &texture2);
    glBindTexture(GL_TEXTURE_2D, texture2);

    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_S, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_WRAP_T, GL_REPEAT);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_LINEAR);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_LINEAR);

    image = SOIL_load_image("99.png", &width, &height, 0, SOIL_LOAD_RGB);
    std::cout << SOIL_last_result() << std::endl;
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGB, width, height, 0, GL_RGB, GL_UNSIGNED_BYTE, image);

    glGenerateMipmap(GL_TEXTURE_2D);
    SOIL_free_image_data(image);
    glBindTexture(GL_TEXTURE_2D, 0);

}

void Draw() {
    glUseProgram(Program); // Устанавливаем шейдерную программу текущей

    glUniform1f(proportionUniformID, proportion);
    glUniform3f(offsetUniformID, offset.x, offset.y, offset.z);
    glUniform3f(glGetUniformLocation(Program, "scale"), scale.x, scale.y, scale.z);

    glActiveTexture(GL_TEXTURE0);
    glBindTexture(GL_TEXTURE_2D, texture1);
    glUniform1i(texture1UniformID, 0);
    glActiveTexture(GL_TEXTURE1);
    glBindTexture(GL_TEXTURE_2D, texture2);
    glUniform1i(texture2UniformID, 1);


    glBindBuffer(GL_ARRAY_BUFFER, VBO); // Подключаем VBO

    // сообщаем OpenGL как он должен интерпретировать вершинные данные.
    glEnableVertexAttribArray(coordAttribID); // Включаем массив атрибутов
    glVertexAttribPointer(coordAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)0);
    
    glEnableVertexAttribArray(textureCoordAttribID); // Включаем массив атрибутов
    glVertexAttribPointer(textureCoordAttribID, 2, GL_FLOAT, GL_FALSE, 32, (GLvoid*)24);

    glEnableVertexAttribArray(colorAttribID); // Включаем массив атрибутов
    glVertexAttribPointer(colorAttribID, 3, GL_FLOAT, GL_FALSE, 32, (GLvoid*)12);
    

    glBindBuffer(GL_ARRAY_BUFFER, 0); // Отключаем VBO
    glDrawArrays(GL_TRIANGLES, 0, 36); // Передаем данные на видеокарту(рисуем)
    
    glDisableVertexAttribArray(coordAttribID); // Отключаем массив атрибутов
    glDisableVertexAttribArray(textureCoordAttribID); // Отключаем массив атрибутов

    glDisableVertexAttribArray(colorAttribID); // Отключаем массив атрибутов

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
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left) && sf::Keyboard::isKeyPressed(sf::Keyboard::X)) offset.x -= 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right) && sf::Keyboard::isKeyPressed(sf::Keyboard::X)) offset.x += 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left) && sf::Keyboard::isKeyPressed(sf::Keyboard::Y)) offset.y -= 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right) && sf::Keyboard::isKeyPressed(sf::Keyboard::Y)) offset.y += 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Left) && sf::Keyboard::isKeyPressed(sf::Keyboard::Z)) offset.z -= 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Right) && sf::Keyboard::isKeyPressed(sf::Keyboard::Z)) offset.z += 0.05f;


        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up) && proportion < 1) proportion += 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down) && proportion > 0) proportion -= 0.05f;

        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up) && scale.x < 2 && sf::Keyboard::isKeyPressed(sf::Keyboard::X)) scale.x += 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down) && scale.x > 0 && sf::Keyboard::isKeyPressed(sf::Keyboard::X)) scale.x -= 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Up) && scale.y < 2 && sf::Keyboard::isKeyPressed(sf::Keyboard::Y)) scale.y += 0.05f;
        if (sf::Keyboard::isKeyPressed(sf::Keyboard::Down) && scale.y > 0 && sf::Keyboard::isKeyPressed(sf::Keyboard::Y)) scale.y -= 0.05f;

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
