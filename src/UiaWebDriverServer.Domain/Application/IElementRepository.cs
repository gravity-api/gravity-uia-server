/*
 * CHANGE LOG - keep only last 5 threads
 * 
 * RESSOURCES
 */
using UiaWebDriverServer.Contracts;

namespace UiaWebDriverServer.Domain.Application
{
    public interface IElementRepository
    {
        (int Status, Element Element) FindElement(string session, LocationStrategy locationStrategy);
        (int Status, Element Element) FindElement(string session, string element, LocationStrategy locationStrategy);
        (int StatusCode, string Text) GetElementText(string session, string element);
        (int StatusCode, string Value) GetElementAttribute(string session, string element, string attribute);
        Element GetElement(string session, string element);
    }
}
